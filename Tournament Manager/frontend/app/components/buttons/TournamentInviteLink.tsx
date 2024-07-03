"use client"

import { Popover, PopoverContent, PopoverTrigger } from "@nextui-org/popover";
import { Button } from "@nextui-org/button";
import { Snippet } from "@nextui-org/snippet";
import React, { useState } from "react";
import { createInvite } from "@/app/api/server/tournaments";

interface TournamentInviteLinkProps {
    id: string
}


export function TournamentInviteLink({ id }: TournamentInviteLinkProps) {
    "use client"

    const [link, setLink] = useState("")
    const [isLoading, setIsLoading] = useState(true)

    const generateInvite = async () => {
        setIsLoading(true);
        const linkRes = await createInvite(id);
        setLink(linkRes.inviteId);
        setIsLoading(false);
    }

    return (
            <Popover placement="bottom">
                <PopoverTrigger>
                    <Button size={"lg"} color={"warning"} onClick={generateInvite}>Generate Invite</Button>
                </PopoverTrigger>
                <PopoverContent>
                    {!isLoading &&
                            <Snippet symbol="" variant="bordered" color={"warning"}>
                                {"http://localhost:3000/tournaments/join/" + link}
                            </Snippet>
                    }
                </PopoverContent>
            </Popover>
    )
}
