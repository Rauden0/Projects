import { Prisma, Tournament } from '@prisma/client';
import { Result } from '@badrap/result';
import prisma from '../../client';
import { TournamentError, TournamentErrorType } from './errors';
import { DbResult } from '../../types';
import { readUserById } from "../../Users/repositories/user.repository";
import { createRoundRobinNodes, createSingleEliminationNodes } from "../../Node/generation/generation";
import { v4 as uuidv4 } from 'uuid';
import QueryString from "qs";
import TournamentWhereInput = Prisma.TournamentWhereInput;

export interface TournamentData
{
    capacity: number;
    title: string;
    startDate: string;
    endDate: string;
    format: string;
    description?: string;
}

export async function createNodeSystem(format: string, tournament: Tournament) {
    switch (format) {
        case "RoundRobin Format":
            return await createRoundRobinNodes(tournament.id, tournament.capacity);
        case "Single Elimination":
            return await createSingleEliminationNodes(0, 1, tournament.capacity / 2, tournament);
        default:
            throw new Error("Unknown format");
    }
}

export async function create(tournamentData:TournamentData, adminSub: string): DbResult<Tournament | null> {
    try {
        const tournament = await prisma.tournament.create({ data: tournamentData });
        const node = await createNodeSystem(tournament.format, tournament);
        if (node) {
            await prisma.tournament.update({
                where: { id: tournament.id },
                data: { nodeId: node.id }
            });
        }

        const userResult = await readUserById(adminSub);

        if (userResult.isOk && userResult.value) {
            await update(tournament.id, { adminUsers: { connect: { id: userResult.value.id } } });
        } else {
            return Result.err(new TournamentError('Error creating tournament. (User problem)', TournamentErrorType.NotFound));
        }

        const resTournament = await prisma.tournament.findFirst({
            where: { id: tournament.id },
            include: {
                rootNode: true,
                adminUsers: true,
                followers: true,
                tournamentNodes: true,
            }
        });

        return Result.ok(resTournament);
    } catch (err) {
        return Result.err(new TournamentError('Error creating tournament.', TournamentErrorType.DatabaseError));
    }
}

export async function readTournamentsPaged(query: QueryString.ParsedQs): Promise<DbResult<Tournament[]>> {
    try {
        const { page = 1, limit = 10 } = query;
        const pageNumber = parseInt(String(page), 10) || 1;
        const pageSize = parseInt(String(limit), 10) || 10;

        const tournaments = await prisma.tournament.findMany({
            skip: (pageNumber - 1) * pageSize,
            take: pageSize,
        });

        return Result.ok(tournaments);
    } catch (error) {
        return Result.err(new TournamentError('Error retrieving tournaments.', TournamentErrorType.DatabaseError));
    }
}

export async function getTotalTournamentsPages(query: QueryString.ParsedQs): Promise<DbResult<number>> {
    try {
        const filter: TournamentWhereInput | undefined = parseTournamentWhereInput(query.where);

        if (filter == undefined)
        {
            throw new TournamentError("Invalid query", TournamentErrorType.DatabaseError);
        }
        const totalTournaments = await prisma.tournament.count({
            where:filter
        });
        const take = parseInt(query.take as string, 10) || 10;
        const totalPages = Math.ceil(totalTournaments / take);
        return Result.ok(totalPages);
    } catch (error) {
        return Result.err(new TournamentError('Error calculating total tournament pages.', TournamentErrorType.DatabaseError));
    }
}

function parseTournamentWhereInput(input: string | string[] | QueryString.ParsedQs | QueryString.ParsedQs[] | undefined | Prisma.TournamentWhereInput): TournamentWhereInput | undefined {
    if (typeof input === 'object' && input !== null) {
        return input as TournamentWhereInput;
    }
    return undefined;
}

export async function update(tournamentId: string, tournamentData: any): DbResult<Tournament | null> {
    try {
        const updatedTournament = await prisma.tournament.update({
            where: { id: tournamentId },
            data: tournamentData,
        });
        return Result.ok(updatedTournament);
    } catch (err) {
        return Result.err(new TournamentError('Error updating tournament.', TournamentErrorType.DatabaseError));
    }
}

export async function deleteTournamentById(tournamentId: string): DbResult<Tournament | null> {
    try {
        const deletedTournament = await prisma.tournament.delete({
            where: { id: tournamentId },
        });
        return Result.ok(deletedTournament);
    } catch (err) {
        return Result.err(new TournamentError('Error deleting tournament.', TournamentErrorType.DatabaseError));
    }
}

export async function readTournamentById(tournamentId: string): DbResult<Tournament | null> {
    try {
        const tournament = await prisma.tournament.findFirst({
            where: { id: tournamentId },
            include: {
                rootNode: true,
                adminUsers: true,
                followers: true,
                tournamentNodes: {
                    select: {
                        predecessorNodeUser2Id: true,
                        predecessorNodeUser1Id: true,
                        userIds: true,
                        scoreA: true,
                        scoreB: true,
                        tournamentId: true,
                        depth: true,
                        tag: true,
                        id: true,
                        successorId: true,
                        users: true,
                        finished: true,
                    }
                }
            },
        });

        if (!tournament) {
            throw new TournamentError('Tournament not found.', TournamentErrorType.NotFound);
        }

        return Result.ok(tournament);
    } catch (err) {
        if (err instanceof TournamentError) {
            return Result.err(err);
        }
        return Result.err(new TournamentError('Error retrieving tournament.', TournamentErrorType.DatabaseError));
    }
}

export async function createTournamentInviteLink(tournamentId: string): Promise<DbResult<Tournament | null>> {
    try {
        const uuid = uuidv4();

        const updatedTournament = await prisma.tournament.update({
            where: { id: tournamentId },
            data: {
                inviteId: uuid,
                inviteIdCreationTime: new Date().toISOString(),
            },
        });

        return Result.ok(updatedTournament);
    } catch (err) {
        return Result.err(new TournamentError('Error creating invite link.', TournamentErrorType.DatabaseError));
    }
}

export async function connectUser(userSub: string, inviteId: string): DbResult<Tournament | null> {
    try {
        const user = await findUser(userSub);
        if (!user) {
            return Result.err(new TournamentError("User not found", TournamentErrorType.NotFound));
        }

        const tournament = await findTournamentByInviteId(inviteId);
        if (!tournament) {
            return Result.err(new TournamentError("No such link was created", TournamentErrorType.NotFound));
        }

        if (!isInviteValid(tournament.inviteIdCreationTime)) {
            return Result.err(new TournamentError("The invite link is too old", TournamentErrorType.Timeout));
        }

        const isUserAlreadyMember = await isUserMemberOfTournament(user.id, tournament.id);
        if (isUserAlreadyMember) {
            return Result.err(new TournamentError("User is already a member of the tournament", TournamentErrorType.Duplicity));
        }

        const node = await connectUserToNode(user.id, tournament.id, tournament.format, tournament.membersIds.length);
        if (!node) {
            return Result.err(new TournamentError("Capacity has been reached", TournamentErrorType.Capacity));
        }

        await addMemberToTournament(user.id, tournament.id);

        const updatedTournament = await findTournamentByInviteId(inviteId);
        return Result.ok(updatedTournament);
    } catch (err) {
        return Result.err(new TournamentError('Error connecting user to tournament.', TournamentErrorType.DatabaseError));
    }
}

export async function isUserMemberOfTournament(userId: string, tournamentId: string): Promise<boolean> {
    const tournament = await prisma.tournament.findUnique({
        where: { id: tournamentId },
        include: { members: { where: { id: userId } } }
    });

    return tournament!.members.length > 0 || false;
}

export async function findUser(userSub: string) {
    return prisma.user.findFirst({ where: { sub: userSub } });
}

export async function findTournamentByInviteId(inviteId: string) {
    return prisma.tournament.findFirst({
        where: { inviteId },
        include: { tournamentNodes: true }
    });
}

export function isInviteValid(inviteCreationTime?: Date| null): boolean {
    if (inviteCreationTime == null)
    {
        return true;
    }
    const FIVE_MINUTES_IN_MS = 5 * 60 * 1000;
    const currentTime = Date.now();
    return inviteCreationTime && (currentTime - new Date(inviteCreationTime).getTime() <= FIVE_MINUTES_IN_MS);
}

export async function addMemberToTournament(userId: string, tournamentId: string) {
    await prisma.tournament.update({
        where: { id: tournamentId },
        data: { members: { connect: { id: userId } } }
    });
}

export async function connectUserToNode(userId: string, tournamentId: string, format: string, currentUserCount: number) {
    switch (format) {
        case "RoundRobin Format":
            return await connectUserRoundRobin(userId, tournamentId, currentUserCount);
        case "Single Elimination":
            return await connectUserSingleElimination(userId, tournamentId);
        default:
            throw new Error("Unknown format");
    }
}

export async function connectUserSingleElimination(userId: string, tournamentId: string) {
    const nodes = await findNodesWithTwoOrFewerUsers(tournamentId);
    const node = nodes.find(node => node.userIds.length < 2);

    if (node) {
        await prisma.node.update({
            where: { id: node.id },
            data: { userIds: { push: userId } }
        });
    }

    return node;
}

export async function connectUserRoundRobin(userId: string, tournamentId: string, currentUserCount: number) {
    const targetNodes = await prisma.node.findMany({
        where: {
            tournamentId,
            tag: {
                contains: `${currentUserCount + 1}`
            }
        }
    });

    for (const node of targetNodes) {
        await prisma.node.update({
            where: { id: node.id },
            data: { userIds: { push: userId } }
        });
    }

    return targetNodes[0];
}

export async function findNodesWithTwoOrFewerUsers(tournamentId: string) {
    return prisma.node.findMany({
        where: {
            tournamentId,
            predecessorNodeUser2: null,
            predecessorNodeUser1: null
        },
        include: { users: true }
    });
}
