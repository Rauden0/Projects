export interface Node {
    id: string,
    depth: number,
    tournamentId: string,
    users: UserData[],
    scoreA: number,
    scoreB: number,
    finished: boolean,
    successorId: string | null,
}

export interface TournamentData {
    id: string,
    title: string,
    capacity: number,
    format: string,
    description: string;
    startDate: string,
    endDate: string,
    tournamentNodes: Node[],
    adminUsers: UserData[],
}

export interface TournamentCreateData {
    title: string,
    capacity: number,
    format: string,
    description: string,
    startDate: string,
    endDate: string,
    adminSub: string,
}

export interface UserData {
    sub: string;
    nickname: string;
    password: string;
    email: string;
    description: string;
    lastLogin: Date;
    registerDate: Date;
    tournamentsManaged: TournamentData[];
    tournamentsFollowed: TournamentData[];
    tournamentsParticipatedIn: TournamentData[];
}

export type InviteLink = {
    inviteId: string,
}
