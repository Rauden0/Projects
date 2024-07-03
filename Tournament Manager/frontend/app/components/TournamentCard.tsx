import { Card, CardBody, CardHeader } from "@nextui-org/card";
import { Divider } from "@nextui-org/divider";
import { Link } from "@nextui-org/link";
import { Chip } from "@nextui-org/chip";

interface TournamentCardProps {
    title: string;
    format?: string;
    id: string;
    description?: string;
    isAdmin?: boolean;
    isAttending?: boolean;
}

export default function TournamentCard({ title, format, id, description, isAdmin, isAttending }: TournamentCardProps) {
    return (
            <Card className="" as={Link} href={`/tournaments/detail/${id}`}>

                <CardHeader className="flex flex-col text-center gap-1">
                    <label className="text-small text-default-500">{format}</label>
                    <label className="text-md font-bold">{title}</label>
                    <label>16 teams</label>
                    <div className="flex flex-row gap-1 items-center">
                        {isAdmin && <Chip color="warning" variant="flat">Admin</Chip>}
                        {isAttending && <Chip color="primary" variant="flat">Attends</Chip>}
                    </div>
                </CardHeader>
                {description &&
                        <>
                            <Divider/>
                            <CardBody className={"text-center"}>
                                <p>{description}</p>
                            </CardBody>
                        </>
                }

            </Card>
    );
}
