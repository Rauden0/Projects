import Link from "next/link";


interface LinkButtonProps {
    text: string;
    link: string;
}

export default function LinkButton({ text, link }: LinkButtonProps) {
    return (
            <>
                <Link className={"flex flex-row bg-cyan-600 justify-center h-min px-[10px] py-[5px] rounded-md"}
                      href={link}>
                    <label className={"text-center"}>{text}</label>
                </Link>
            </>
    )
}
