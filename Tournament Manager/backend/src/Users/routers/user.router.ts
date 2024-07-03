import express from 'express';
import {
    createUser,
    deleteUser,
    followTournament,
    getUserById,
    getUsers,
    updateUser,
} from '../controllers/user.controller';

const userRouter = express.Router();
userRouter.get('/', getUsers);
userRouter.get('/:id', getUserById);
userRouter.post('/', createUser);
userRouter.patch('/:id', updateUser);
userRouter.delete('/:id', deleteUser);
userRouter.patch('/follow/:id', followTournament);

export default userRouter;
