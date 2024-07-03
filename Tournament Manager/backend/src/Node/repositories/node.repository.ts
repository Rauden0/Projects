import { Node } from '@prisma/client';
import { Result } from '@badrap/result';
import prisma from '../../client';
import { NodeError, NodeErrorType } from './errors';
import { DbResult } from '../../types';

const advance = async (nodeId: string, userId: string): DbResult<Node | null> => {
    try {
        const currentNode = await prisma.node.findUnique({
            where: { id: nodeId },
            include: { users: true }
        });

        if (!currentNode) {
            return Result.err(new NodeError('Node not found.', NodeErrorType.NotFound));
        }

        if (currentNode.users.length !== 2) {
            return Result.err(new NodeError('They are not both users on this node.', NodeErrorType.DatabaseError));
        }

        const nodeData = currentNode.users[1]?.sub === userId
            ? { scoreA: 0, scoreB: 1, finished: true }
            : { scoreA: 1, scoreB: 0, finished: true };

        const updatedNode = await prisma.node.update({
            where: { id: nodeId },
            data: nodeData
        });

        const successorId = currentNode.successorId;
        if (!successorId) {
            return Result.err(new NodeError('No successor node found.', NodeErrorType.DatabaseError));
        }

        const succNode = await prisma.node.findUnique({
            where: { id: successorId },
            include: { users: true }
        });

        if (!succNode || succNode.users.length >= 2) {
            return Result.err(new NodeError('Cannot advance, the successor node is full.', NodeErrorType.DatabaseError));
        }

        await prisma.node.update({
            where: { id: successorId },
            data: {
                users: { connect: { sub: userId } }
            }
        });

        return Result.ok(updatedNode);
    } catch (err) {
        console.error('Error advancing node:', err);
        return Result.err(new NodeError('Error moving user to successor node.', NodeErrorType.DatabaseError));
    }
};

const readNodeById = async (nodeId: string): DbResult<Node | null> => {
    try {
        const node = await prisma.node.findUnique({
            where: { id: nodeId }
        });

        if (!node) {
            throw new NodeError('Node not found.', NodeErrorType.NotFound);
        }

        return Result.ok(node);
    } catch (err) {
        if (err instanceof NodeError) {
            return Result.err(err);
        }
        console.error('Error retrieving node by ID:', err);
        return Result.err(new NodeError('Error retrieving node.', NodeErrorType.DatabaseError));
    }
};

const readPaged = async (query: any): DbResult<Node[]> => {
    try {
        const { page = 1, limit = 10 } = query;
        const pageNumber = parseInt(page, 10) || 1;
        const pageSize = parseInt(limit, 10) || 10;

        const nodes = await prisma.node.findMany({
            skip: (pageNumber - 1) * pageSize,
            take: pageSize
        });

        return Result.ok(nodes);
    } catch (error) {
        console.error('Error retrieving paged nodes:', error);
        return Result.err(new NodeError('Error retrieving nodes.', NodeErrorType.DatabaseError));
    }
};

const getTotalPages = async (query: any): DbResult<number> => {
    try {
        const totalNodes = await prisma.node.count({
            where: query.where
        });
        const totalPages = Math.ceil(totalNodes / (query.take || 10));

        return Result.ok(totalPages);
    } catch (error) {
        console.error('Error calculating total pages:', error);
        return Result.err(new NodeError('Error calculating total pages.', NodeErrorType.DatabaseError));
    }
};

export {
    advance,
    readNodeById,
    readPaged,
    getTotalPages
};
