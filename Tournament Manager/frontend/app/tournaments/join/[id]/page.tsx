"use server"

import { fetchUser } from "@/app/api/server/user";
import { getSession } from "@auth0/nextjs-auth0";
import { Login } from "@/app/components/buttons/Login";
import React from "react";
import { JoinButton } from "@/app/components/buttons/JoinButton";


export default async function Home({ params }: { params: { id: string } }) {
    "use server"

    const session = await getSession();
    const user = await fetchUser(session?.user.sub);

    return (
            <div className={"p-6 flex flex-col items-center text-center text-6xl justify-center h-screen"}>
                {!user &&
                        <div>
                            <h1> You need to be logged in to join a tournament! </h1>
                            <Login/>
                        </div>
                }
                {user &&
                        <JoinButton userSub={user.sub} inviteId={params.id}/>
                }
            </div>
    );
}
