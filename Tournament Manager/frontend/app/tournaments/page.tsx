import { fetchUser } from "@/app/api/server/user";
import { getSession } from "@auth0/nextjs-auth0";
import { TournamentList } from "@/app/components/TournamentList";

export default async function Home() {

    const session = await getSession();

    const user = await fetchUser(session?.user.sub);
    const managed = user?.tournamentsManaged;
    const followed = user?.tournamentsFollowed;
    const attended = user?.tournamentsParticipatedIn;

    return (
            <>
                <TournamentList managed={managed} followed={followed} attended={attended}/>
            </>
    );
}
