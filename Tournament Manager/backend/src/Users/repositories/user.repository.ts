import { User } from '@prisma/client';
import { Result } from '@badrap/result';
import prisma from '../../client';
import { UserError, UserErrorType } from './errors';
import { DbResult } from '../../types';

export interface UserData
{
    nickname?: string|undefined,
    description?: string | undefined,
    tournamentsManaged?: string[] | undefined,
    tournamentsManagedIds?: string[] | undefined,
    tournamentsFollowed?: string[] | undefined,
    tournamentsFollowedIds?: string[] | undefined ,
    tournamentsParticipatedIn?: string[] | undefined,
    tournamentsParticipatedIds?: string[] | undefined
}
async function isDuplicate(userSub: string) {
    return prisma.user.findFirst({
        where: { sub: userSub },
    });
}

const create = async (userData: User): DbResult<User | null> => {
    try {
        if (await isDuplicate(userData.sub)) {
            throw new UserError('User with the same Sub was found', UserErrorType.DatabaseError);
        }
        await prisma.user.create({
            data: userData,
        });
        return Result.ok<User | null, UserError>(userData);
    } catch (err: any) {
        if (err instanceof UserError) {
            return Result.err<UserError, User>(err);
        } else {
            return Result.err<UserError, User>(new UserError(err.message, UserErrorType.DatabaseError));
        }
    }
};

const update = async (userSub: string, userData:UserData): DbResult<User | null> => {
    try {
        const dataToUpdate = {
            nickname: userData.nickname ?? "",
            description: userData.description ?? "",
            userData,
        }
        const updatedUser = await prisma.user.update({
            where: { sub: userSub },
            data: dataToUpdate,
        });
        return Result.ok<User | null, UserError>(updatedUser);
    } catch (err) {
        return Result.err<UserError, User | null>(new UserError('Error updating user.', UserErrorType.DatabaseError));
    }
};

const deleteUserById = async (userSub: string): DbResult<User | null> => {
    try {
        const deletedUser = await prisma.user.delete({
            where: { sub: userSub },
        });
        return Result.ok<User | null, UserError>(deletedUser);
    } catch (err) {
        return Result.err<UserError, User | null>(new UserError('Error deleting user.', UserErrorType.DatabaseError));
    }
};

const readUserById = async (userSub: string): DbResult<User | null> => {
    try {
        const user = await prisma.user.findUnique({
            where: { sub: userSub },
            include: {
                tournamentsFollowed: true,
                tournamentsManaged: true,
                tournamentsParticipatedIn: true,
            },
        });
        if (!user) {
            throw new UserError('User not found.', UserErrorType.NotFound);
        }
        return Result.ok<User | null, UserError>(user);
    } catch (err) {
        if (err instanceof UserError) {
            return Result.err<UserError, User | null>(err);
        }
        return Result.err<UserError, User | null>(new UserError('Error retrieving user.', UserErrorType.DatabaseError));
    }
};

const readPaged = async (query: any): Promise<DbResult<User[]>> => {
    try {
        const { page = 1, limit = 10 } = query;
        const pageNumber = parseInt(page, 10) || 1;
        const pageSize = parseInt(limit, 10) || 10;

        const users = await prisma.user.findMany({
            skip: (pageNumber - 1) * pageSize,
            take: pageSize,
        });
        return Result.ok<User[], UserError>(users);
    } catch (error) {
        return Result.err<UserError, User[]>(new UserError('Error retrieving users.', UserErrorType.DatabaseError));
    }
};

const getTotalPages = async (query: any): Promise<DbResult<number>> => {
    try {
        const totalUsers = await prisma.user.count({
            where: query.where,
        });
        const totalPages = Math.ceil(totalUsers / query.take);

        return Result.ok<number, UserError>(totalPages);
    } catch (error) {
        return Result.err<UserError, number>(new UserError('Error calculating total pages.', UserErrorType.DatabaseError));
    }
};

const follow = async (userSub: string, tournamentId: string): DbResult<User | null> => {
    try {
        await prisma.tournament.update({
            where: { id: tournamentId },
            data: { followers: { connect: { sub: userSub } } },
        });
        const updatedUser = await prisma.user.findFirst({
            where: { sub: userSub },
            include: {
                tournamentsFollowed: true,
                tournamentsManaged: true,
                tournamentsParticipatedIn: true,
            },
        });
        return Result.ok<User | null, UserError>(updatedUser);
    } catch (err) {
        return Result.err<UserError, User | null>(new UserError('Error updating user.', UserErrorType.DatabaseError));
    }
};

export {
    create,
    update,
    deleteUserById,
    readUserById,
    readPaged,
    getTotalPages,
    follow
};
