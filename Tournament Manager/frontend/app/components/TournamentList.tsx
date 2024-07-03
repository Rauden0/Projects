import TournamentCard from "@/app/components/TournamentCard";
import { TournamentData } from "@/app/api/server/types";

interface TournamentListProps {
    managed?: TournamentData[];
    followed?: TournamentData[];
    attended?: TournamentData[];
}


export function TournamentList({ managed, followed, attended }: TournamentListProps) {
    return (
            <div className={"flex flex-col space-y-[15px] p-[15px]"}>
                {managed &&
                        <>
                            {managed.map(tournament => {
                                return (
                                        <TournamentCard id={tournament.id}
                                                        key={tournament.id}
                                                        title={tournament.title}
                                                        description={tournament.description}
                                                        format={tournament.format}
                                                        isAdmin
                                        />
                                )
                            })}
                        </>
                }
                {followed &&
                        <>
                            {followed.map(tournament => {
                                return (
                                        <TournamentCard id={tournament.id}
                                                        key={tournament.id}
                                                        title={tournament.title}
                                                        description={tournament.description}
                                                        format={tournament.format}
                                        />
                                )
                            })}
                        </>
                }
                {attended &&
                        <>
                            {attended.map(tournament => {
                                return (
                                        <TournamentCard id={tournament.id}
                                                        key={tournament.id}
                                                        title={tournament.title}
                                                        description={tournament.description}
                                                        format={tournament.format}
                                                        isAttending
                                        />
                                )
                            })}
                        </>
                }
            </div>
    )
}
