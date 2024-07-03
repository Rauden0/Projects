import { getSession } from "@auth0/nextjs-auth0";
import { fetchUser } from "@/app/api/server/user";
import { EditSubmit } from "@/app/components/buttons/EditSubmit";

export default async function Page() {
    const session = await getSession()
    const user = await fetchUser(session?.user.sub)


    return (
            <>
                {user &&
                        <EditSubmit user={user}/>
                }
            </>
    );
}

