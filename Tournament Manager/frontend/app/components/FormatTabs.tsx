"use client"

import { Tab, Tabs } from "@nextui-org/tabs";
import { Card, CardBody } from "@nextui-org/card";
import React, { ReactElement } from "react";

interface FormatTabsProps {
    Graph: ReactElement;
    Table: ReactElement;
}


export function FormatTabs({ Graph, Table }: FormatTabsProps) {
    return (
            <div className="flex w-full flex-col gap-3 text-gray-500">
                <label className={"text-3xl"}>Format</label>
                <Tabs aria-label="Options">
                    <Tab key="graph" title="Graph">
                        <Card>
                            <CardBody>
                                {Graph}
                            </CardBody>
                        </Card>
                    </Tab>
                    <Tab key="table" title="Table">
                        <Card>
                            <CardBody>
                                {Table}
                            </CardBody>
                        </Card>
                    </Tab>
                    <Tab key="hide" title="Hide">
                    </Tab>
                </Tabs>
            </div>
    )
}
