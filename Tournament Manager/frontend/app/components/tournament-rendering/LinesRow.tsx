interface LinesProps {
    nodeCount: number;
    capacity: number;
}


export function LinesRow({ nodeCount, capacity }: LinesProps) {
    const chars = (nodeCount / 2)
    const length = ((Math.log2(capacity) * ((Math.log2(capacity) / 2) * 7 + 1))) / nodeCount;

    return (
            <div className={"flex flex-col justify-around gap-y-12"}>
                {Array.from({ length: chars }, (_, index) => (
                        <div className={"flex flex-col text-end"}>
                            <label key={0} className={"-mb-[2px]"}>{`┐`}</label>
                            {Array.from({ length: length }, (_, index) => (
                                    <label key={index} className={"-my-[2px]"}>{"│"}</label>
                            ))}
                            <label key={index} className={"-my-[2px]"}>{"├"}</label>
                            {Array.from({ length: length }, (_, index) => (
                                    <label key={index} className={"-my-[2px]"}>{"│"}</label>
                            ))}
                            <label key={0} className={"-mt-[2.5px]"}>{`┘`}</label>
                        </div>
                ))}
            </div>
    )
}
