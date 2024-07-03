import express from 'express';
import {
    connectUserToTournament,
    createInviteLink,
    createTournament,
    deleteTournament,
    getTournamentById,
    getTournaments,
    updateTournament,
} from '../controllers/tournament.controller';

const tournamentRouter = express.Router();
tournamentRouter.get('/', getTournaments);
tournamentRouter.get('/:id', getTournamentById);
tournamentRouter.post('/', createTournament);
tournamentRouter.patch('/:id', updateTournament);
tournamentRouter.delete('/:id', deleteTournament);
tournamentRouter.get('/invite/:id', createInviteLink);
tournamentRouter.patch('/join/:inviteId', connectUserToTournament)
export default tournamentRouter;
