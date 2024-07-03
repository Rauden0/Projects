"use client";

import { Node, UserData } from "@/app/api/server/types";
import React, { useState } from "react";
import { BracketRow } from "@/app/components/tournament-rendering/BracketRow";
import { LinesRow } from "@/app/components/tournament-rendering/LinesRow";
import { advanceNode } from "@/app/api/server/nodes";

interface FormatViewerProps {
    initialNodes: Node[];
    isAdmin: boolean;
}


function nodesToBracket(nodes: Node[]): Map<number, Node[]> {
    return nodes.reduce((acc, node) => {
        if (!acc.has(node.depth)) {
            acc.set(node.depth, []);
        }
        acc.get(node.depth)?.push(node);
        return acc;
    }, new Map<number, Node[]>());
}


export function ClientSideFormatViewer({ initialNodes, isAdmin }: FormatViewerProps) {
    const [nodes, setNodes] = useState<Node[]>(initialNodes);
    const [nodesByDepth, setNodesByDepth] = useState(nodesToBracket(nodes));

    const updateBrackets = (teamId: string, _: string, id: string) => {
        if (!isAdmin) return;

        const source = nodes.find(n => n.id === id);
        if (!source || source.finished || !source.users[0] || !source.users[1]) return;

        const target = nodes.find(n => n.id === source.successorId);
        const team: UserData | undefined = source.users.find(u => u.sub === teamId);

        source.finished = true;
        source.users[0].sub === teamId ? source.scoreA = 1 : source.scoreB = 1;

        if (target) {
            if (!target.users[0]) {
                advanceNode(id, teamId);
                target.users[0] = team!;
            } else if (!target.users[1]) {
                advanceNode(id, teamId);
                target.users[1] = team!;
            }
            setNodes(nodes.map(node => node.id === target.id ? target : node));
        }

        advanceNode(id, teamId);
        setNodes(nodes.map(node => node.id === source.id ? source : node));
        setNodesByDepth(nodesToBracket(nodes));
    };

    return (
            <div className="flex flex-row px-6">
                {Array.from(nodesByDepth.entries()).reverse().map(([depth, brackets], index) => (
                        <React.Fragment key={depth}>
                            <BracketRow brackets={brackets} onClick={updateBrackets}/>
                            {index < nodesByDepth.size - 1 &&
                                    <LinesRow
                                            capacity={nodesByDepth.get(nodesByDepth.size - 1)!.length}
                                            nodeCount={brackets.length}
                                    />
                            }
                        </React.Fragment>
                ))}
            </div>
    );
}
