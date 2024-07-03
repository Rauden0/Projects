import { faker } from '@faker-js/faker/locale/cz';
import prisma from '../client';
import { DbResult } from "../types";
import { Tournament, User } from "@prisma/client";
import { TournamentError, TournamentErrorType } from '../Tournaments/repositories/errors';
import { Result } from '@badrap/result';
import {
    addMemberToTournament,
    connectUserToNode,
    createNodeSystem,
    update
} from "../Tournaments/repositories/tournament.repository";
import { readUserById } from "../Users/repositories/user.repository";

const create = async (tournamentData: {
    endDate: Date;
    format: string;
    adminUsersIds: (string)[];
    description: string;
    title: string;
    startDate: Date;
    status: string;
    capacity: number
}, adminSub: string): DbResult<Tournament | null> => {
    try {
        const tournament = await prisma.tournament.create({ data: tournamentData });
        const node = await createNodeSystem(tournamentData.format, tournament);

        if (node) {
            await prisma.tournament.update({
                where: { id: tournament.id },
                data: { nodeId: node.id }
            });
        }

        const user: Result<User | null> = await readUserById(adminSub);

        if (user.isOk && user.value) {
            await update(tournament.id, { adminUsers: { connect: { id: user.value.id } } });
        } else {
            return Result.err<TournamentError, Tournament | null>(new TournamentError('Error creating tournament. (User problem)', TournamentErrorType.DatabaseError));
        }

        const resTournament = await prisma.tournament.findFirst({
            where: { id: tournament.id },
            include: {
                rootNode: true,
                adminUsers: true,
                followers: true,
                tournamentNodes: true,
            }
        });

        return Result.ok<Tournament | null, TournamentError>(resTournament);
    } catch (err) {
        return Result.err<TournamentError, Tournament | null>(new TournamentError('Error creating tournament.', TournamentErrorType.DatabaseError));
    }
};


async function userSeeding() {
    return prisma.user.createMany({
        data: Array.from({ length: 50 }).map(() => ({
            nickname: faker.internet.userName(),
            sub: faker.datatype.uuid(),
            email: faker.internet.email(),
            description: faker.lorem.sentence(),
            lastLogin: faker.date.recent(),
            registerDate: faker.date.past(),
        })),
    });
}

const hardcodedTitles = [
    "Winter Championship 2024",
    "Summer Showdown 2024",
    "Spring Invitational 2024",
    "Autumn Open 2024",
    "New Year Bash 2024",
    "Champions League 2024",
    "Pro Series 2024",
    "Elite Tournament 2024",
    "Ultimate Clash 2024",
    "Final Battle 2024"
];

const hardcodedDescriptions :string[] = [
    "Join us for an exciting winter tournament!",
    "Compete in the summer showdown and prove your skills.",
    "The spring invitational is open for all competitive players.",
    "Welcome to the autumn open tournament!",
    "Celebrate the new year with our special bash tournament.",
    "Enter the champions league and play with the best.",
    "The pro series is for top-tier competitors only.",
    "Show your elite skills in this exclusive tournament.",
    "Prepare for the ultimate clash of titans.",
    "This is the final battle of the year. Don't miss out!"
];

async function tournamentSeeding(users: User[]) {
    const capacities = [2, 4, 8, 16, 32];
    const formats = ["RoundRobin Format", "Single Elimination"];
    for (let i = 0; i < 10; i++) {
        const randomUserIndex = faker.datatype.number({ min: 0, max: users.length - 1 });
        const randomUser = users[randomUserIndex];
        const description = hardcodedDescriptions[i];
        const title = hardcodedTitles[i];

        if (!title || !description) {
            console.error(`Title or description missing for index ${i}`);
            continue;
        }

        const tournamentData = {
            title,
            format: faker.helpers.arrayElement(formats),
            status: faker.helpers.arrayElement(['Active', 'Nonactive']),
            description,
            startDate: faker.date.recent(),
            endDate: faker.date.future(),
            capacity: faker.helpers.arrayElement(capacities),
            adminUsersIds: [randomUser!.id],
        };

        await create(tournamentData, randomUser!.sub);
    }
}

async function assignUsersToTournaments(users: User[], tournaments: Tournament[]) {
    for (const tournament of tournaments) {
        const userCount = tournament.capacity;
        const usersToAssign = faker.helpers.arrayElements(users, userCount);
        for (const user of usersToAssign) {
            await addMemberToTournament(user.id, tournament.id);
            await connectUserToNode(user.id, tournament.id, "Single Elimination", tournament.membersIds.length);
        }
    }
}

async function main() {
    await userSeeding();
    const users = await prisma.user.findMany();
    await tournamentSeeding(users);
    const tournaments = await prisma.tournament.findMany();
    await assignUsersToTournaments(users, tournaments);
}

main()
    .then(() => console.log('Seeding completed successfully.'))
    .catch((error) => console.error('Error during seeding:', error))
    .finally(async () => {
        await prisma.$disconnect();
    });
