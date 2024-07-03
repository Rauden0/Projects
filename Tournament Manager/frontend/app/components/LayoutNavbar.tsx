'use client'

import React from "react";
import { RocketLaunchIcon } from "@heroicons/react/24/outline";
import {
    Navbar,
    NavbarBrand,
    NavbarContent,
    NavbarItem,
    NavbarMenu,
    NavbarMenuItem,
    NavbarMenuToggle
} from "@nextui-org/navbar";
import { Button } from "@nextui-org/button";
import { Link } from "@nextui-org/link";
import { useUser } from "@auth0/nextjs-auth0/client";

export default function LayoutNavbar() {
    const [isMenuOpen, setIsMenuOpen] = React.useState(false);
    const { user } = useUser();

    const menuItems = [
        { label: "My Tournaments", href: "/tournaments" },
        { label: "Create Tournament", href: "/tournaments/create" },
        { label: "My Profile", href: `/user/detail/${user?.sub}` },
        { label: "All Tournaments", href: `/tournaments/all` },];

    return (<>
        {user &&
                <Navbar onMenuOpenChange={setIsMenuOpen}>
                    <NavbarContent>
                        <NavbarMenuToggle
                                aria-label={isMenuOpen ? "Close menu" : "Open menu"}
                                className="sm:hidden"
                        />
                        <NavbarBrand>
                            <RocketLaunchIcon className={"h-[42px] w-[42px] "}/>
                            <Link className="font-bold text-inherit" href={"/"}>Fast Tournaments</Link>
                        </NavbarBrand>
                    </NavbarContent>

                    <NavbarContent className="hidden sm:flex gap-4" justify="center">
                        {menuItems.map(({ label, href }, index) => (<NavbarMenuItem key={`${label}-${index}`}>
                            <Link
                                    color="foreground"
                                    href={href}
                            >
                                {label}
                            </Link>
                        </NavbarMenuItem>))}
                    </NavbarContent>

                    <NavbarContent justify="end">
                        <NavbarItem>
                            {!user && <Button as={Link} color="primary" href="/api/auth/login" variant="flat">
                                Sign Up / Login
                            </Button>}
                            {user && <Button as={Link} color="danger" href="/api/auth/logout" variant="flat">
                                Logout
                            </Button>}
                        </NavbarItem>
                    </NavbarContent>

                    <NavbarMenu>
                        {menuItems.map(({ label, href }, index) => (<NavbarMenuItem key={`${label}-${index}`}>
                            <Link
                                    color={"foreground"}
                                    className="w-full"
                                    href={href}
                                    size={"lg"}
                            >
                                {label}
                            </Link>
                        </NavbarMenuItem>))}
                    </NavbarMenu>
                </Navbar>}
    </>)
}
