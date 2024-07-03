"use client";

import React from "react";
import { parseDate } from "@internationalized/date";
import { DateRangePicker } from "@nextui-org/date-picker";

interface ClientSideDateRangePickerProps {
    label: string;
    startDate: string;
    endDate: string;
}

const ClientSideDateRangePicker: React.FC<ClientSideDateRangePickerProps> = ({ label, startDate, endDate }) => {
    const parsedStartDate = parseDate(startDate.split("T")[0]);
    const parsedEndDate = parseDate(endDate.split("T")[0]);

    return (
            <DateRangePicker
                    classNames={{
                        label: "text-3xl",
                        input: "text-1xl"
                    }}
                    label={label}
                    isReadOnly
                    defaultValue={{
                        start: parsedStartDate,
                        end: parsedEndDate,
                    }}
                    color={"default"}
                    className="max-w-xs"
                    isDisabled
                    labelPlacement={"outside"}
                    size={"lg"}
            />
    );
};

export default ClientSideDateRangePicker;
