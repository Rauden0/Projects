"use client"

import { usePathname } from "next/navigation";
import { Popover, PopoverContent, PopoverTrigger } from "@nextui-org/popover";
import { Button } from "@nextui-org/button";
import { Snippet } from "@nextui-org/snippet";
import React from "react";

export function TournamentLink() {
    const pathname = usePathname()

    return (
            <Popover placement="bottom">
                <PopoverTrigger>
                    <Button size={"lg"}>Spectate Link</Button>
                </PopoverTrigger>
                <PopoverContent>
                    <Snippet symbol="" variant="bordered">{"http://localhost:3000" + pathname}</Snippet>
                </PopoverContent>
            </Popover>
    )
}
