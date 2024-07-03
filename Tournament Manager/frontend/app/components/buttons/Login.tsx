import { Link } from "@nextui-org/link";
import { Button } from "@nextui-org/button";
import React from "react";

export function Login() {
    return (
            <Button className={"h-12 text-2xl font-bold md:h-32 md:text-6xl md:rounded-3xl"} as={Link} color="primary"
                    href="/api/auth/login" variant="shadow">
                Sign Up!
            </Button>
    )
}
