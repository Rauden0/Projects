"use client"

import { patchUserToTournament } from "@/app/api/server/tournaments";
import { Button } from "@nextui-org/button";
import React, { useState } from "react";

export interface JoinButtonProps {
    inviteId: string;
    userSub: string;
}


export function JoinButton({ inviteId, userSub }: JoinButtonProps) {
    const [text, setText] = useState("Error")
    const [disabled, setDisabled] = useState(false);
    const [error, setError] = useState(false);


    const handleClick = async (event: React.MouseEvent<HTMLButtonElement>) => {
        const status = await patchUserToTournament(inviteId, userSub)

        console.log("Status code: " + status);

        if (status == 200) {
        } else {
            setError(true)
            switch (status) {
                case 420:
                    setText("Tournament is at full capacity");
                    break;
                case 421:
                    setText("Invite has expired");
                    break;
                case 422:
                    setText("You are already attending the tournament");
                    break;
                case 404:
                    setText("This link is not associated with any tournament");
                    break;
                case 500:
                    setText("The Server ran into an issue");
                    break;
            }
        }

        setDisabled(true);
    }

    return (

            <>
                {!disabled && !error &&
                        <Button size={"lg"} onClick={handleClick}>
                            Join tournament
                        </Button>
                }
                {disabled && !error &&
                        <Button size={"lg"} disabled color={"success"}>
                            You have joined the tournament!
                        </Button>
                }
                {disabled && error &&
                        <Button size={"lg"} disabled color={"danger"}>
                            {text}
                        </Button>
                }
            </>

    )
}
