import { Card, CardBody } from "@nextui-org/card";
import { Button } from "@nextui-org/button";
import React from "react";
import { Divider } from "@nextui-org/divider";

import { Node, UserData } from "@/app/api/server/types";

export type Team = {
    id: string,
    name: string,
    score: number,
}

export interface RowProps {
    user?: UserData
    score: number
    onClick: (id: string | undefined, team: string | undefined, button: React.MouseEvent<HTMLButtonElement>) => void,
}

export interface BracketProps {
    bracket: Node,
    onClick: (teamId: string | undefined, team: string | undefined, nodeId: string) => void,
}


function BracketRow({ user, score, onClick }: RowProps) {

    return (
            <Button className={`flex flex-row justify-between w-full bg-transparent ${score === 1 ? "bg-green-500" +
                    " text-white" : ""}`} onClick={(event) =>
                    onClick(user?.sub, user?.nickname, event)} color={"default"}>
                <label>{user?.nickname || "TBD"}</label>
                <label>{score}</label>
            </Button>
    )
}


export default function Bracket({ bracket, onClick }: BracketProps) {

    const buttonClick = (id: string | undefined, team: string | undefined, nodeId: string, event: React.MouseEvent<HTMLButtonElement>) => {
        onClick(id, team, nodeId);
    }

    return (
            <>
                <Card>
                    <CardBody className={"flex flex-col justify-center w-full gap-y-1"}>
                        <BracketRow user={bracket.users[0]} score={bracket.scoreA} onClick={(id, team, event) =>
                                bracket.finished || buttonClick(id, team, bracket.id, event)}/>
                        <Divider/>
                        <BracketRow user={bracket.users[1]} score={bracket.scoreB} onClick={(id, team, event) =>
                                bracket.finished || buttonClick(id, team, bracket.id, event)}/>
                    </CardBody>
                </Card>
            </>
    )
}
