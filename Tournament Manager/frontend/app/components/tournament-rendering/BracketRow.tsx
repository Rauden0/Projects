import Bracket from "@/app/components/BracketData";

import { Node } from "@/app/api/server/types"

interface BracketRowProps {
    brackets: Node[]
    onClick: (teamId: string, team: string, nodeId: string) => void,
}


export function BracketRow({ brackets, onClick }: BracketRowProps) {

    return (
            <div className={"flex flex-col justify-around gap-12"}>
                {brackets.map((bracket, index) => (
                        <div key={index}>
                            {/*// @ts-ignore*/}
                            <Bracket bracket={bracket} onClick={onClick}/>
                        </div>
                ))}
            </div>
    )
}
