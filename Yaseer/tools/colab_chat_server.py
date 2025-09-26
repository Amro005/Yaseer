import socket
import threading
import time
import requests
import nest_asyncio
import uvicorn
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Dict, Any
from pycloudflared import try_cloudflare


def free_port() -> int:
    s = socket.socket()
    s.bind(("", 0))
    p = s.getsockname()[1]
    s.close()
    return p


nest_asyncio.apply()

app = FastAPI()
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


class ChatIn(BaseModel):
    message: str
    contextId: str | None = None
    slots: Dict[str, Any] | None = None


def next_prompt(slots: Dict[str, Any]) -> str:
    order = [
        ("city", "ما المدينة؟"),
        ("hospital", "ما المستشفى؟"),
        ("department", "ما القسم؟ (Dentist/Eye/Orthopedic/General)"),
        ("checkup", "ما نوع الفحص؟ (Basic/Full Body/Dental Cleaning)"),
        ("date", "ما التاريخ؟ (YYYY-MM-DD)"),
        ("time", "ما الوقت؟ (HH:MM)"),
        ("needsTransport", "هل تحتاج نقل؟ (Yes/No)"),
        ("transportAddress", "عنوان الالتقاط"),
        ("payment", "طريقة الدفع؟"),
        ("extras", "خدمات إضافية؟"),
        ("contact", "رقم الهاتف أو البريد؟"),
    ]
    for key, prompt in order:
        if key not in slots or slots.get(key) in (None, "", "skip"):
            if key == "transportAddress" and str(slots.get("needsTransport", "")).lower().startswith("n"):
                slots["transportAddress"] = ""
                continue
            return prompt
    return ""


def update_slots(slots: Dict[str, Any], message: str) -> Dict[str, Any]:
    ask = [
        "city",
        "hospital",
        "department",
        "checkup",
        "date",
        "time",
        "needsTransport",
        "transportAddress",
        "payment",
        "extras",
        "contact",
    ]
    for k in ask:
        if k not in slots or slots.get(k) in (None, "", "skip"):
            slots[k] = message.strip()
            break
    return slots


@app.post("/chat/send")
def chat_send(data: ChatIn):
    slots = dict(data.slots or {})
    slots = update_slots(slots, data.message)
    prompt = next_prompt(slots)
    if prompt == "":
        t = "Yes" if str(slots.get("needsTransport", "")).lower().startswith("y") else "No"
        reply = (
            "Appointment confirmed.\n"
            f"City: {slots.get('city','')}\n"
            f"Hospital: {slots.get('hospital','')}\n"
            f"Department: {slots.get('department','')}\n"
            f"Checkup: {slots.get('checkup','')}\n"
            f"Date & Time: {slots.get('date','')} {slots.get('time','')}\n"
            f"Transport: {t}\n"
            f"Transport details: {slots.get('transportAddress','')}\n"
            f"Additional services: {slots.get('extras','')}\n"
            f"Payment: {slots.get('payment','')}\n"
            f"Contact: {slots.get('contact','')}\n"
        )
        return {"reply": reply, "done": True, "slots": slots}
    return {"reply": prompt, "done": False, "slots": slots}


def run_uvicorn(port: int) -> None:
    uvicorn.run(app, host="0.0.0.0", port=port, log_level="info")


def start_server_with_tunnel() -> None:
    port = free_port()
    threading.Thread(target=run_uvicorn, args=(port,), daemon=True).start()
    time.sleep(3)
    pub = try_cloudflare(port)
    base = pub.tunnel
    print("PUBLIC_URL:", base)
    url = f"{base}/chat/send"
    wait = 2.0
    for i in range(15):
        try:
            r = requests.post(url, json={"message": "Amman", "slots": {}}, timeout=20)
            print("READY", r.status_code, r.headers.get("content-type"))
            if r.headers.get("content-type", "").startswith("application/json"):
                print(r.json())
            else:
                print(r.text[:200])
            break
        except Exception as e:
            print("RETRY", i + 1, "ERROR", e)
            time.sleep(wait)
            wait = wait * 1.6 if wait < 12 else 12
    else:
        print("DNS not ready. Try again with:", url)


if __name__ == "__main__":
    start_server_with_tunnel()


