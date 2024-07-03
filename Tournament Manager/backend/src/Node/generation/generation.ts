import prisma from '../../client';
import { Node, Tournament } from '@prisma/client';

export async function createSingleEliminationNodes(
    depth: number,
    current: number,
    totalCount: number,
    tournament: Tournament,
    left?: boolean,
    parentNode?: Node
): Promise<Node | null> {
    if (totalCount <= 0) {
        return null;
    }

    try {
        const nodeData = {
            tag: 'idk',
            depth,
            finished: false,
            scoreA: 0,
            scoreB: 0,
            ...(parentNode && { successorId: parentNode.id })
        };

        const newNode = await prisma.node.create({
            data: nodeData,
        });

        await prisma.tournament.update({
            where: { id: tournament.id },
            data: {
                tournamentNodes: {
                    connect: { id: newNode.id }
                }
            }
        });

        if (parentNode) {
            await prisma.node.update({
                where: { id: parentNode.id },
                data: left
                    ? { predecessorNodeUser1Id: newNode.id }
                    : { predecessorNodeUser2Id: newNode.id }
            });
        }

        await Promise.all([
            createSingleEliminationNodes(depth + 1, current * 2, totalCount - current, tournament, true, newNode),
            createSingleEliminationNodes(depth + 1, current * 2, totalCount - current, tournament, false, newNode)
        ]);

        return newNode;
    } catch (error) {
        console.error('Error creating single elimination nodes:', error);
        throw new Error('Failed to create single elimination nodes');
    }
}

export async function createRoundRobinNodes(tournamentId: string, totalTeams: number): Promise<Node | null> {
    if (totalTeams < 2) return null;

    try {
        let totalNodes = 0;
        let newNode: Node | null = null;

        for (let i = 0; i < totalTeams - 1; i++) {
            for (let j = i + 1; j < totalTeams; j++) {
                const nodeData  = {
                    tag: `Match ${i + 1} vs ${j + 1}`,
                    depth: totalNodes,
                    scoreA: 0,
                    scoreB: 0,
                    finished: false,
                    tournamentId: tournamentId
                };

                newNode = await prisma.node.create({
                    data: nodeData,
                });

                await prisma.tournament.update({
                    where: { id: tournamentId },
                    data: {
                        tournamentNodes: {
                            connect: { id: newNode.id }
                        }
                    }
                });

                totalNodes++;
            }
        }
        return newNode;
    } catch (error) {
        console.error('Error creating round robin nodes:', error);
        throw new Error('Failed to create round robin nodes');
    }
}
