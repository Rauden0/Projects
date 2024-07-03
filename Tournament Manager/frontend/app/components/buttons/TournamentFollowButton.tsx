"use client"

import { Button } from "@nextui-org/button";
import React, { useState } from "react";
import { followTournament } from "@/app/api/server/user";

interface TournamentFollowButtonProps {
    userId?: string;
    tournamentId: string
}


export function TournamentFollowButton({ userId, tournamentId }: TournamentFollowButtonProps) {
    const [following, setFollowing] = useState(false)

    return (
            <>
                {userId && tournamentId && !following &&
                        <Button color={"primary"} onClick={() => {
                            setFollowing(true);
                            followTournament(userId, tournamentId)
                        }}>
                            Follow this Tournament
                        </Button>
                }
                {following &&
                        <Button color={"success"}>
                            You follow this tournament!
                        </Button>
                }
            </>
    )
}
