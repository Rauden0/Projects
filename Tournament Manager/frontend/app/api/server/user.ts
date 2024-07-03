import axios, { AxiosResponse } from 'axios';
import { UserData } from "@/app/api/server/types";


export async function fetchUser(id: string): Promise<UserData | null> {
    return axios.get(process.env.API_URL + `/user/${id}`)
        .then((response: AxiosResponse) => {
            console.log(response.data)
            return response.data as UserData;
        })
        .catch(error => {
            console.log(error);
            return null;
        });
}


export async function patchUser(id: string, formData: FormData): Promise<UserData> {

    const jsonForm = { nickname: formData.get("nickname"), description: formData.get("description") };
    console.log(id);
    console.log(jsonForm);

    return axios.patch("http://localhost:8080" + `/user/${id}`, jsonForm)
        .then((response: AxiosResponse) => {
            const userData: UserData = response.data;
            userData.lastLogin = new Date(userData.lastLogin);
            userData.registerDate = new Date(userData.registerDate);
            return userData;
        })
        .catch((error: any) => {
            throw new Error(error.message);
        });
}


export async function followTournament(userId: string, tournamentId: string): Promise<UserData> {

    return axios.patch("http://localhost:8080" + `/user/follow/${userId}`, { id: tournamentId })
        .then(res => {
            console.log(res);
            return res.status;
        })
        .catch((err) => {
            console.log(err.toJSON().status);
            return err.toJSON().status
        })
}


export async function deleteUser(id: string): Promise<UserData> {
    return axios.delete(process.env.API_URL + `/user/${id}`)
        .then((response: AxiosResponse) => {
            const userData: UserData = response.data;
            userData.lastLogin = new Date(userData.lastLogin);
            userData.registerDate = new Date(userData.registerDate);
            return userData;
        })
        .catch((error: any) => {
            throw new Error(error.message);
        });
}
