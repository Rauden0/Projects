'use client'

import React from "react";
import { Dropdown, DropdownItem, DropdownMenu, DropdownTrigger } from "@nextui-org/dropdown";
import { Button } from "@nextui-org/button";


const options = [
    'Single Elimination',
    'Double Elimination',
    'Swiss Format',
    'RoundRobin Format',
]

// @ts-ignore
export default function FormatDropdown({ onFormatChange }) {
    const [selectedFormat, setSelectedFormat] = React.useState("");

    const handleFormatSelect = (format: string) => {
        setSelectedFormat(format);
        onFormatChange(format); // Pass selected format back to parent component
    };

    return (
            <Dropdown className={"dark w-screen"}>
                <DropdownTrigger className={"dark w-full "}>
                    <Button
                            variant="solid"
                            className="capitalize h-12"
                    >
                        <p className={"absolute left-3 top-2 text-xs"}>Format</p>
                        {selectedFormat || "Select Format"}
                    </Button>
                </DropdownTrigger>
                <DropdownMenu
                        aria-label="Single selection example"
                        variant="flat"
                        disallowEmptySelection
                        selectionMode="single"
                >
                    {options.map((option, index) => (
                            <DropdownItem key={index} onClick={() => handleFormatSelect(option)}>
                                {option}
                            </DropdownItem>
                    ))}
                </DropdownMenu>
            </Dropdown>
    );
}
