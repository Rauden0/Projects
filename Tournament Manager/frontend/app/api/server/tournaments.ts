'use server'


import axios from "axios";
import { InviteLink, TournamentCreateData, TournamentData } from "@/app/api/server/types";

export async function createTournament(formData: TournamentCreateData) {
    console.log(formData)

    axios.post(process.env.API_URL + '/tournament', formData)
        .then(console.log)
        .catch(err => console.log(err))
}


export async function fetchTournament(id: string): Promise<TournamentData | null> {
    return await axios.get(process.env.API_URL + `/tournament/${id}`)
        .then(res => {
            console.log(res.data);
            return res.data
        })
        .catch(console.log)
}


export async function fetchAllTournaments(): Promise<TournamentData[] | null> {
    return await axios.get("http://localhost:8080" + `/tournament`)
        .then(res => {
            console.log(res.data.items);
            return res.data.items
        })
        .catch(console.log)
}


export async function createInvite(id: string): Promise<InviteLink> {
    return await axios.get(process.env.API_URL + `/tournament/invite/${id}`)
        .then(res => {
            console.log(res.data);
            return res.data;
        })
        .catch(console.log)
}


export async function patchUserToTournament(inviteId: string, userSub: string): Promise<number | Error> {
    return await axios.patch("http://localhost:8080" + `/tournament/join/${inviteId}`, { userSub: userSub })
        .then(res => {
            console.log(res);
            return res.status;
        })
        .catch((err) => {
            console.log(err.toJSON().status);
            return err.toJSON().status
        })
}


// @ts-ignore
export async function patchTournament(id: string, formData: FormData): Promise<TournamentData> {

}


// @ts-ignore
export async function deleteTournament(id: string): Promise<TournamentData> {

}
