"use client"
import { getKeyValue, Table, TableBody, TableCell, TableColumn, TableHeader, TableRow } from "@nextui-org/table";
import React from "react";
import { Node, UserData } from "@/app/api/server/types";

const columns = [
    {
        key: "name",
        label: "NAME",
    },
    {
        key: "score",
        label: "SCORE",
    },
];

export interface UserTableProps {
    nodes: Node[]
}


interface UserScore {
    key: string,
    name: string,
    score: number
}


export function UserTable({ nodes }: UserTableProps) {

    function aggregateAndSortUsers(nodes: Node[]): UserScore[] {
        const userScores: Map<string, UserScore> = new Map();

        nodes.forEach(node => {
            const userA: UserData | undefined = node.users[0];
            const userB: UserData | undefined = node.users[1];

            if (userA != null && !userScores.has(userA.sub)) {
                userScores.set(userA.sub, { key: userA.sub, name: userA.nickname, score: 0 });
            }
            if (userB != null && !userScores.has(userB?.sub)) {
                userScores.set(userB.sub, { key: userB.sub, name: userB.nickname, score: 0 });
            }

            if (userScores.has(userA?.sub)) {
                userScores.get(userA.sub)!.score += node.scoreA;
            }
            if (userScores.has(userB?.sub)) {
                userScores.get(userB.sub)!.score += node.scoreB;
            }

        });

        return Array.from(userScores.values()).sort((a, b) => b.score - a.score);
    }


    const scores = aggregateAndSortUsers(nodes);

    return (
            <Table>
                <TableHeader columns={columns}>
                    {(column) => <TableColumn key={column.key}>{column.label}</TableColumn>}
                </TableHeader>
                <TableBody items={scores}>
                    {(item) => (
                            <TableRow key={item.key}>
                                {(columnKey) => <TableCell>{getKeyValue(item, columnKey)}</TableCell>}
                            </TableRow>
                    )}
                </TableBody>
            </Table>
    )
}
