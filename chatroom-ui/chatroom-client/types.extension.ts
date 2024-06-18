import { ChatMsg } from "./types.gen";

export const GetTextContent: (msg: ChatMsg) => string | undefined = (msg: ChatMsg) => {
    // if parts contains only one text part, return that
    if (msg.parts?.length === 1 && msg.parts[0].textPart != null) {
        return msg.parts[0].textPart;
    }
    else
    {
        return undefined
    }
}