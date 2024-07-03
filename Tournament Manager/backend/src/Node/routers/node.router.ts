import express from 'express';
import { advanceNode, getNodeById, getNodes, } from '../controllers/node.controller';

const nodeRouter = express.Router();
nodeRouter.get('/', getNodes);
nodeRouter.get('/:id', getNodeById);
nodeRouter.patch('/:id', advanceNode);
export default nodeRouter;
