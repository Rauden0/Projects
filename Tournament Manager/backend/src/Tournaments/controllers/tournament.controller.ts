import { Request, Response } from 'express';
import { Result } from '@badrap/result';
import { Tournament } from '@prisma/client';
import { parseRequest } from '../../utils';
import {
    connectUser,
    create,
    createTournamentInviteLink,
    deleteTournamentById,
    getTotalTournamentsPages,
    readTournamentById,
    readTournamentsPaged,
    update,
} from '../repositories/tournament.repository';

import {
    ConnectUserSchema,
    CreateTournamentSchema,
    DeleteTournamentSchema,
    InviteLinkSchema,
    Tournament as TournamentSchema,
    TournamentRequestSchema,
    UpdateTournamentSchema,
} from '../validationSchemas/validationSchemas';
import { TournamentError, TournamentErrorType } from '../repositories/errors';

function getErrorCode(error: TournamentError) {
    switch (error.type) {
        case TournamentErrorType.Capacity:
            return 420;
        case TournamentErrorType.Timeout:
            return 421;
        case TournamentErrorType.Duplicity:
            return 422;
        case TournamentErrorType.NotFound:
            return 404;
        case TournamentErrorType.DatabaseError:
            return 500;
    }
}

async function handleResponse(res: Response, result: Result<Tournament | Tournament[] | null>) {
    if (result.isOk) {
        return res.status(200).json(result.value);
    }
    return res.status(getErrorCode(new TournamentError(result.error.message,TournamentErrorType.DatabaseError))).json(result.error.message);
}

export async function getTournaments(req: Request, res: Response) {
    try {
        const request = await parseRequest(TournamentSchema, req, res);
        if (!request) return;

        const tournamentQuery = req.query;

        const tournamentsResult = await readTournamentsPaged(tournamentQuery);
        const totalPagesResult = await getTotalTournamentsPages(tournamentQuery);

        if (tournamentsResult.isOk && totalPagesResult.isOk) {
            const { value: tournaments } = tournamentsResult;

            res.status(200).json({
                items: tournaments,
                pagination: {
                    currentPage: tournamentQuery.page,
                    totalPages: totalPagesResult.value,
                },
            });
        } else {
            await handleResponse(res, tournamentsResult);
        }
    } catch (error) {
        res.status(500).json(error);
    }
}

export async function getTournamentById(req: Request, res: Response) {
    const request = await parseRequest(TournamentRequestSchema, req, res);
    if (!request) return;
    const tournamentId = request.params.id;
    const result = await readTournamentById(tournamentId);
    await handleResponse(res, result);
}

export async function createTournament(req: Request, res: Response) {
    const request = await parseRequest(CreateTournamentSchema, req, res);
    if (!request) return;
    const format = request.body.format ?? '';
    const description = request.body.description ?? '';
    const { adminSub, ...tournamentFields } = request.body;
    const tournamentData = {
        ...tournamentFields,
        format:format,
        description:description,
        capacity: parseInt(request.body.capacity, 10),
    };
    const result = await create(tournamentData, adminSub);
    await handleResponse(res, result);
}

export async function updateTournament(req: Request, res: Response) {
    const request = await parseRequest(UpdateTournamentSchema, req, res);
    if (!request) return;

    const id = request.params.id!;
    const result = await update(id, request.body);
    await handleResponse(res, result);
}

export async function deleteTournament(req: Request, res: Response) {
    const request = await parseRequest(DeleteTournamentSchema, req, res);
    if (!request) return;

    const tournamentId = request.params.id!;
    const result = await deleteTournamentById(tournamentId);
    await handleResponse(res, result);
}

export async function createInviteLink(req: Request, res: Response) {
    const request = await parseRequest(InviteLinkSchema, req, res);
    if (!request) return;

    const tournamentId = request.params.id!;
    const result = await createTournamentInviteLink(tournamentId);
    await handleResponse(res, result);
}

export async function connectUserToTournament(req: Request, res: Response) {
    const request = await parseRequest(ConnectUserSchema, req, res);
    if (!request) return;

    const tournamentInviteId = request.params.inviteId;
    const result = await connectUser(request.body.userSub, tournamentInviteId);
    await handleResponse(res, result);
}
