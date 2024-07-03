'use client'

import FormatDropdown from "@/app/components/FormatDropdown";
import { Input, Textarea } from "@nextui-org/input";
import { Button } from "@nextui-org/button";
import { useEffect, useState } from "react";
import { createTournament } from "@/app/api/server/tournaments";
import { CalendarDate, RangeCalendar, RangeValue } from "@nextui-org/calendar";
import { getLocalTimeZone, today } from "@internationalized/date";
import { useUser } from "@auth0/nextjs-auth0/client";
import { useRouter } from "next/navigation";


export default function Home() {
    const { user, error, isLoading } = useUser();
    const router = useRouter()

    const [formData, setFormData] = useState({
        title: "",
        capacity: 0,
        format: "",
        description: "",
        startDate: new Date().toISOString(),
        endDate: new Date().toISOString(),
        adminSub: user?.sub || "",
    });

    useEffect(() => {
        if (!isLoading) {
            setFormData((prevFormData) => ({
                ...prevFormData, adminSub: user?.sub || ""
            }));
        }
    }, [isLoading, user]);

    const handleChange = (e: { target: { name: string; value: string; }; }) => {
        const { name, value } = e.target;
        setFormData((prevFormData) => ({
            ...prevFormData, [name]: value
        }));
    };

    const handleFormatChange = (format: string) => {
        setFormData((prevFormData) => ({
            ...prevFormData, format: format
        }));
    };

    const handleDateChange = (date: RangeValue<CalendarDate>) => {
        setFormData((prevFormData) => ({
            ...prevFormData,
            startDate: new Date(date.start.toString()).toISOString(),
            endDate: new Date(date.end.toString()).toISOString()
        }));
    };

    const handleSubmit = (e: { preventDefault: () => void; }) => {
        e.preventDefault();

        createTournament(formData).then(r => r);
        router.push("http://localhost:3000/tournaments")
    };

    return (
            !isLoading && user && (
                    <form className="flex flex-col p-[15px] items-center justify-between h-full gap-[15px]"
                          onSubmit={handleSubmit}>
                        <Input
                                label="Title"
                                name="title"
                                placeholder="Super cool tournament name"
                                type="text"
                                onChange={handleChange}
                        />
                        <Input
                                label="Capacity"
                                name="capacity"
                                placeholder="1"
                                type="number"
                                onChange={handleChange}
                        />
                        <FormatDropdown onFormatChange={handleFormatChange}/>
                        <Textarea
                                maxRows={3}
                                label="Description"
                                name="description"
                                placeholder="Tell us about this Tournament!"
                                onChange={handleChange}
                        />

                        <div className="flex gap-x-4">
                            <RangeCalendar
                                    aria-label="Date (Uncontrolled)"
                                    minValue={today(getLocalTimeZone())}
                                    defaultValue={{
                                        start: today(getLocalTimeZone()),
                                        end: today(getLocalTimeZone()).add({ weeks: 1 }),
                                    }}
                                    onChange={handleDateChange}
                            />
                        </div>

                        <Button type="submit" color="success" isLoading={false}>
                            Submit
                        </Button>
                    </form>
            )
    );
}
