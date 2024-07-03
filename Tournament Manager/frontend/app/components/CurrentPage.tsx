'use client'

import React from "react";
import { usePathname } from "next/navigation";
import { Divider } from "@nextui-org/divider";

function getPageName(pathname: string) {
    if (pathname === "/tournaments") {
        return "My Tournaments";
    } else if (pathname.startsWith("/user/detail")) {
        return "User Detail";
    } else if (pathname.startsWith("/tournaments/detail")) {
        return "Tournaments Detail";
    } else if (pathname.startsWith("/tournaments/create")) {
        return "Tournaments Create";
    } else if (pathname.startsWith("/user/edit")) {
        return "User Edit";
    }
    return ""
}


export default function CurrentPage() {
    const pathname = usePathname();
    const pageName = getPageName(pathname);

    return (
            <>
                {pageName !== "" &&
                        <div className={"flex flex-col items-center px-4 text-center p-[15px] gap-3"}>
                            <h1 className={"text-xl font-bold"}>{getPageName(pathname)}</h1>
                            <Divider/>
                        </div>
                }
            </>


    );
}
