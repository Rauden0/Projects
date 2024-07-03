import { z } from 'zod';

const options = [
    'Single Elimination',
    'Double Elimination',
    'Swiss Format',
    'RoundRobin Format',
];
export const Tournament = z.object({
    id: z.string().optional(),
    type: z.string().optional(),
    members: z.array(z.string()).optional(),
    membersIds: z.array(z.string()).optional(),
    followers: z.array(z.string()).optional(),
    followersIds: z.array(z.string()).optional(),
    adminUsers: z.array(z.string()).optional(),
    adminUsersIds: z.array(z.string()).optional(),
    link: z.string().url().optional(),
    status: z.string().optional(),
    description: z.string().optional(),
    startDate: z.date().optional(),
    endDate: z.date().optional(),
    capacity: z.number().int().optional(),
});

export const CreateTournamentSchema = z.object({
    body: z.object(
        {
            format: z.union([
                z.literal(options[0]),
                z.literal(options[1]),
                z.literal(options[2]),
                z.literal(options[3]),
            ]),
            title: z.string(),
            description: z.string().optional(),
            startDate: z.string().datetime(),
            endDate: z.string().datetime(),
            adminSub: z.string(),
            capacity: z.string().regex(/^\d+$/).refine(value => {
                const numericValue = parseInt(value);
                return numericValue % 2 == 0 && numericValue <= 32 && numericValue > 0;
            }, {
                message: 'Capacity must be a valid integer between 0 and 32 and must be divisible by 2'
            })
        }),
});

export const UpdateTournamentSchema = z.object({
    params: z.object(
        {
            id: z.string(),
        }),
    body: z.object({
        title: z.string().optional(),
        description: z.string().optional(),
        startDate: z.string().datetime().optional(),
        endDate: z.string().datetime().optional(),
    })
});

export const DeleteTournamentSchema = z.object({
    params: z.object({
        id: z.string(),
    }),
});

export const InviteLinkSchema = z.object({
    params: z.object(
        {
            id: z.string(),
        }),
    body: z.object({
        adminSub: z.string(),
    })
});
export const TournamentRequestSchema = z.object({
    params: z.object({
        id: z.string(),
    }),
});

export const ConnectUserSchema = z.object({
    params: z.object({
        inviteId: z.string(),
    }),
    body: z.object({
        userSub: z.string(),
    }),
});
