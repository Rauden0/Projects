import axios, { AxiosResponse } from "axios";
import { getSession } from "@auth0/nextjs-auth0";
import { fetchUser } from "@/app/api/server/user";
import { Button } from "@nextui-org/button";
import { Link } from "@nextui-org/link";
import React from "react";

async function CreateUser(user: any) {
    console.log(user)

    if (user == null) {
        return;
    }

    return axios.post(process.env.API_URL + `/user`, user)
            .then((response: AxiosResponse) => {
                const userData = response.data;
                userData.lastLogin = new Date(userData.lastLogin);
                userData.registerDate = new Date(userData.registerDate);
                console.log(response.data)
                return userData;
            })
            .catch((error: any) => {
                console.log(error);
            });
}


export default async function Page() {
    const session = await getSession();

    const userInfo = await CreateUser(session?.user);
    const user = await fetchUser(session?.user.sub);

    return (
            <>
                {user &&
                        <div className={"flex flex-col justify-around text-center h-full p-8"}>
                            <label className={"text-4xl"}>Welcome back <span
                                    className={"text-purple-400"}>{user.nickname}</span></label>
                            <label className={"text-2xl"}>What do you wanna do today?</label>
                            <div className={"flex flex-col gap-4 justify-between"}>
                                <Button as={Link} color={"primary"} href={"/tournaments"}>My Tournaments</Button>
                                <Button as={Link} color={"secondary"} href={"/tournaments/create"}>Create
                                    Tournament</Button>
                                <Button as={Link} color={"primary"} href={`/user/detail/${user.sub}`}>My
                                    Profile</Button>
                            </div>
                        </div>
                }
                {!user &&

                        <div className={"flex flex-col gap-16 md:gap-32 justify-center bg-cover bg-center h-screen p-8"}
                             style={{
                                 backgroundImage: "url('https://nextui-docs-v2.vercel.app/images/hero-card-complete.jpeg')",
                             }}>
                            <div className="flex flex-col ">
                                <h1 className={"text-6xl md:text-9xl font-bold"}>Welcome!</h1>
                                <p className={"text-4xl md:text-6xl font-semibold"}>
                                    Let's make and manage a tournament with us, <span
                                        className="text-primary">FAST</span> and <span
                                        className="text-primary">EASY</span>!
                                </p>
                            </div>
                            <div>
                                <Button className={"h-12 text-2xl font-bold md:h-32 md:text-6xl md:rounded-3xl"}
                                        as={Link} color="primary" href="/api/auth/login" variant="shadow">
                                    Sign Up!
                                </Button>
                            </div>
                        </div>

                }
            </>
    );
}
