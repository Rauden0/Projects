import { z } from 'zod';

export const User = z.object({
    id: z.string().optional(),
    userName: z.string().optional(),
    password: z.string().optional(),
    email: z.string().email().optional(),
    tournamentsManaged: z.array(z.string()).optional(),
    tournamentsManagedIds: z.array(z.string()).optional(),
    tournamentsFollowed: z.array(z.string()).optional(),
    tournamentsFollowedIds: z.array(z.string()).optional(),
    tournamentsParticipatedIn: z.array(z.string()).optional(),
    tournamentsParticipatedIds: z.array(z.string()).optional(),
    description: z.string().optional(),
    lastLogin: z.date().optional(),
    registerDate: z.date().optional(),
});

export const CreateUserSchema = z.object({
    body: z.object({
        sub: z.string(),
        nickname: z.string(),
        email: z.string().email().optional(),
    })
});

export const UpdateUserSchema = z.object({
    params: z.object({
        id: z.string(),
    }),
    body: z.object({
        nickname: z.string().optional(),
        description: z.string().optional(),
        tournamentsManaged: z.array(z.string()).optional(),
        tournamentsManagedIds: z.array(z.string()).optional(),
        tournamentsFollowed: z.array(z.string()).optional(),
        tournamentsFollowedIds: z.array(z.string()).optional(),
        tournamentsParticipatedIn: z.array(z.string()).optional(),
        tournamentsParticipatedIds: z.array(z.string()).optional(),
    }),
});

export const DeleteUserSchema = z.object({
    params: z.object(
        {
            id: z.string(),
        })
});


export const UserRequestSchema = z.object({
    params: z.object({
        id: z.string(),
    }),
});
export const FollowTournamentSchema =
    z.object({
        params: z.object({
            id: z.string(),
        }),
        body: z.object({
            id: z.string(),
        }),
    });
