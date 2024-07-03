import { Request, Response } from 'express';
import { Result } from '@badrap/result';
import { User } from '@prisma/client';
import { parseRequest } from '../../utils';
import {
    create,
    deleteUserById,
    follow,
    getTotalPages,
    readPaged,
    readUserById,
    update,
} from '../repositories/user.repository';
import {
    CreateUserSchema,
    DeleteUserSchema,
    FollowTournamentSchema,
    UpdateUserSchema,
    User as UserSchema,
    UserRequestSchema,
} from '../validationSchemas/validationSchemas';
import mapRequestBodyToDatabaseBodyCreate from '../mappers';

async function handleResponse(res: Response, result: Result<User | User[] | null>, errCode: number) {
    if (result.isOk) {
        return res.status(200).json(result.value);
    }
    return res.status(errCode).json(result.error.message);
}

export async function getUsers(req: Request, res: Response) {
    try {
        const request = await parseRequest(UserSchema, req, res);
        if (!request) return;

        const userQuery = req.query;
        const usersResult = await readPaged(userQuery);
        const totalPagesResult = await getTotalPages(userQuery);

        if (usersResult.isOk && totalPagesResult.isOk) {
            const { value: users } = usersResult;

            res.status(200).json({
                items: users,
                pagination: {
                    currentPage: userQuery.page,
                    totalPages: totalPagesResult.value,
                },
            });
        } else {
            await handleResponse(res, usersResult, 404);
        }
    } catch (error) {
        res.status(500).json(error);
    }
}

export async function getUserById(req: Request, res: Response) {
    const request = await parseRequest(UserRequestSchema, req, res);
    if (!request) return;

    const userSub = request.params.id;
    const result = await readUserById(userSub);
    await handleResponse(res, result, 404);
}

export async function createUser(req: Request, res: Response) {
    const request = await parseRequest(CreateUserSchema, req, res);
    if (!request) return;

    const userData = await mapRequestBodyToDatabaseBodyCreate(request.body);
    const result = await create(userData);
    await handleResponse(res, result, 500);
}

export async function updateUser(req: Request, res: Response) {
    const request = await parseRequest(UpdateUserSchema, req, res);
    if (!request) return;

    const id = request.params.id;
    const result = await update(id, request.body);
    await handleResponse(res, result, 404);
}

export async function deleteUser(req: Request, res: Response) {
    const parseResult = await parseRequest(DeleteUserSchema, req, res);
    if (!parseResult) {
        res.status(404).json({ message: 'User ID not provided or invalid.' });
        return;
    }

    const userId = parseResult.params.id;
    const result = await deleteUserById(userId);
    await handleResponse(res, result, 404);
}

export async function followTournament(req: Request, res: Response) {
    const parseResult = await parseRequest(FollowTournamentSchema, req, res);
    if (!parseResult) {
        res.status(404).json({ message: 'User ID or Tournament ID not provided or invalid.' });
        return;
    }

    const userSub = parseResult.params.id;
    const result = await follow(userSub, parseResult.body.id);
    await handleResponse(res, result, 404);
}
