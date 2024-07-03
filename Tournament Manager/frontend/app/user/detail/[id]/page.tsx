import { Avatar } from "@nextui-org/avatar";
import { ScrollShadow } from "@nextui-org/scroll-shadow";
import { Button } from "@nextui-org/button";
import { Link } from "@nextui-org/link";
import { getSession } from "@auth0/nextjs-auth0";
import { fetchUser } from "@/app/api/server/user";
import { TournamentList } from "@/app/components/TournamentList";

const convertPipe = (str: string) => str.replace("|", "%7C");

export default async function Page({ params }: { params: { id: string } }) {
    const user = await fetchUser(params.id);
    const session = await getSession();
    const managed = user?.tournamentsManaged;
    const followed = user?.tournamentsFollowed;
    const attended = user?.tournamentsParticipatedIn;
    const isOwnProfile = session && convertPipe(session.user.sub as string) === params.id;

    return (
            <>{user &&
                    <div className={"flex flex-col gap-6 w-full h-screen p-6"}>
                        <div className={"flex flex-row gap-6 w-full justify-around"}>
                            <h1 className={"text-2xl font-bold"}>
                                Welcome to my profile!
                                <br/> My name is {user.nickname}!
                            </h1>
                            {isOwnProfile &&
                                    <Button as={Link} color={"warning"} href={`/user/edit/${session.user.sub}`}>
                                        Edit
                                    </Button>
                            }
                        </div>


                        <div className={"flex flex-row justify-around gap-6"}>
                            <div className={"flex flex-col p-6 gap-6"}>
                                <Avatar isBordered radius="sm" size={"lg"}
                                        src="https://i.pravatar.cc/150?u=a04258a2462d826712d"/>
                                <p>
                                    I am here since!
                                    <br/>{user.registerDate.toString()}
                                </p>
                                <p>
                                    I have been last seen!
                                    <br/>{user.lastLogin.toString()}
                                </p>
                            </div>

                            <ScrollShadow className="w-max h-[200px]" orientation={"vertical"}>
                                <div>
                                    <p className={"text-wrap break-words"}>
                                        {user.description}
                                    </p>
                                </div>
                            </ScrollShadow>

                        </div>
                        <TournamentList managed={managed} attended={attended} followed={[]}/>
                    </div>}
            </>
    )
}
