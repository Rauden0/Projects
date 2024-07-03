"use server"

import React from "react";
import { fetchTournament } from "@/app/api/server/tournaments";
import { FormatTabs } from "@/app/components/FormatTabs";
import { ClientSideFormatViewer } from "@/app/components/tournament-rendering/ClientSideFormatViewer";
import { Textarea } from "@nextui-org/input";
import ClientSideDateRangePicker from "@/app/components/ClientSideDateRangePicker";
import { TournamentLink } from "@/app/components/buttons/TournamentLink";
import { TournamentInviteLink } from "@/app/components/buttons/TournamentInviteLink";
import { fetchUser } from "@/app/api/server/user";
import { getSession } from "@auth0/nextjs-auth0";
import { TournamentFollowButton } from "@/app/components/buttons/TournamentFollowButton";
import { UserTable } from "@/app/components/tournament-rendering/UserTable";


export default async function Page({ params }: { params: { id: string } }) {
    const tournament = await fetchTournament(params.id);
    const session = await getSession();
    const user = await fetchUser(session?.user.sub)

    const isAdmin = !!tournament?.adminUsers.find(admin => admin.sub == user?.sub);
    // const isAdmin = true;

    return (
            <>
                <div className={"flex flex-col h-max bg-black p-6 gap-y-6"}>
                    {tournament != null &&
                            <>
                                <h1 className={"text-center text-4xl font-bold"}>{tournament.title}</h1>
                                <div className={"flex flex-col md:flex-row items-center text-center" +
                                        " justify-center gap-2"}>
                                    <TournamentFollowButton tournamentId={tournament.id} userId={user?.sub}/>
                                    <TournamentLink/>
                                    {isAdmin && <TournamentInviteLink id={params.id}/>}
                                </div>
                                <Textarea
                                        classNames={{
                                            label: "text-3xl",
                                            input: "text-2xl"
                                        }}
                                        isDisabled
                                        label="Description"
                                        labelPlacement="outside"
                                        defaultValue={`${tournament.description}`}
                                        className="text-6xl"
                                />
                                <div className={"flex flex-col justify-center items-center gap-y-2 text-center"}>
                                    <ClientSideDateRangePicker
                                            label={"Duration"}
                                            startDate={tournament.startDate}
                                            endDate={tournament.endDate}
                                    />
                                </div>

                                <FormatTabs
                                        Graph={<ClientSideFormatViewer initialNodes={tournament.tournamentNodes!}
                                                                       isAdmin={isAdmin}/>}
                                        Table={<UserTable nodes={tournament.tournamentNodes!}/>}
                                />
                            </>
                    }
                </div>
            </>
    )
}
