export enum TournamentErrorType {
    NotFound = 'NOT_FOUND',
    DatabaseError = 'DATABASE_ERROR',
    Capacity = "CAPACITY",
    Timeout = "TIMEOUT",
    Duplicity = "Duplicity"
}

export class TournamentError extends Error {
    constructor(message: string, public type: TournamentErrorType) {
        super(message);
        this.name = 'TournamentError';
    }
}
