import { Request, Response } from 'express';
import { Result } from '@badrap/result';
import { Node } from '@prisma/client';
import { parseRequest } from '../../utils';

import { advance, getTotalPages, readNodeById, readPaged, } from '../repositories/node.repository';
import {
    Node as NodeSchema,
    NodeRequestSchema,
    UpdateNodeAdvanceSchema,
} from '../validationSchemas/validationSchemas';

async function handleResponse(res: Response, result: Result<Node | Node[] | null>, errCode: number) {
    if (result.isOk) {
        return res.status(200).json(result.value);
    }
    return res.status(errCode).json({ error: result.error.message });
}

export async function getNodes(req: Request, res: Response) {
    try {
        const request = await parseRequest(NodeSchema, req, res);
        if (!request) return;

        const nodeQuery = req.query;

        const nodesResult = await readPaged(nodeQuery);
        const totalPagesResult = await getTotalPages(nodeQuery);

        if (nodesResult.isOk && totalPagesResult.isOk) {
            const { value: Nodes } = nodesResult;

            res.status(200).json({
                items: Nodes,
                pagination: {
                    currentPage: nodeQuery.page,
                    totalPages: totalPagesResult.value,
                },
            });
        } else {
            await handleResponse(res, nodesResult, 404);
        }
    } catch (error) {
        res.status(500).json(error);
    }
}


export async function getNodeById(req: Request, res: Response) {
    const request = await parseRequest(NodeRequestSchema, req, res);
    if (!request) return;
    const NodeSub = request.params.id;
    const result = await readNodeById(NodeSub);
    await handleResponse(res, result, 404);
}


export async function advanceNode(req: Request, res: Response) {
    const request = await parseRequest(UpdateNodeAdvanceSchema, req, res);
    if (!request) return;
    const id = request.params.id;
    const result = await advance(id, request.body.id);
    await handleResponse(res, result, 404);
}
