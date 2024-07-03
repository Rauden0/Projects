import axios from "axios";

export async function advanceNode(nodeId: string, userId: string) {

    console.log(nodeId, userId)
    return await axios.patch("http://localhost:8080" + `/nodes/${nodeId}`, { id: userId })
        .then(res => {
            console.log(res);
        })
        .catch((err) => {
            console.log(err);
        })
}
