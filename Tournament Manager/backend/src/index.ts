import express from 'express';
import cors from 'cors';
import { config } from 'dotenv';
import { env } from 'process';
import userRouter from './Users/routers/user.router';
import tournamentRouter from './Tournaments/routers/tournament.router';
import nodeRouter from "./Node/routers/node.router";

config();
const app = express();

const port = 8080;

app.use(cors());

app.use(express.json());

app.use(express.urlencoded({ extended: true }));

app.use('/user', userRouter);
app.use('/tournament', tournamentRouter);
app.use('/nodes', nodeRouter);

app.use((_req, res) => {
    res.status(404).send('Not found');
});

if (env.NODE_ENV !== 'test') {
    app.listen(port, () => {
        console.log(`[${new Date().toISOString()}] RESTful API for tournament is listening on port ${port}`);
    });
}
