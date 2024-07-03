import { z } from 'zod';

export const Node = z.object({
    id: z.string().optional(),
    tag: z.string().optional(),
    email: z.string().email().optional(),
    description: z.string().optional(),
    lastLogin: z.date().optional(),
    registerDate: z.date().optional(),
});
export const UpdateNodeAdvanceSchema = z.object({
    params: z.object({
        id: z.string(),
    }),
    body: z.object({
        id: z.string(),
    }),
});
export const NodeRequestSchema = z.object({
    params: z.object({
        id: z.string(),
    }),
});
