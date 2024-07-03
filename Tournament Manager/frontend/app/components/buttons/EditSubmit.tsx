"use client"

import { Button } from "@nextui-org/button";
import { Input, Textarea } from "@nextui-org/input";
import { useRouter } from "next/navigation";
import { patchUser } from "@/app/api/server/user";
import { UserData } from "@/app/api/server/types";

interface EditSubmitProps {
    user: UserData;
}


export function EditSubmit({ user }: EditSubmitProps) {
    const router = useRouter()


    const onSubmit = (data: FormData) => {
        patchUser(user.sub, data).then(console.log)
        router.replace(`http://localhost:3000/user/detail/${user.sub}`);
    }

    return (
            <>
                <form className={"flex flex-col p-[15px] items-center justify-between h-full gap-[15px]"}
                      action={onSubmit}>
                    <Input label="Nickname" name={"nickname"} placeholder={user!.nickname} type="text"/>
                    <Textarea label="Description" name={"description"} placeholder={user!.description} type="text"/>
                    <Button type={"submit"} color={"success"} isLoading={false}>Submit</Button>
                </form>
            </>
    )
}
