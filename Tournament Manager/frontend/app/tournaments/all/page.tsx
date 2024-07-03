import { TournamentList } from "@/app/components/TournamentList";
import { fetchAllTournaments } from "@/app/api/server/tournaments";

export default async function Home() {

    const tournaments = await fetchAllTournaments();


    return (
            <>
                {tournaments &&
                        <TournamentList managed={[]} followed={tournaments} attended={[]}/>
                }
            </>
    );
}
