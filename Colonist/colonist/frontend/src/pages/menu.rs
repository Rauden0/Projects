use leptos::*;
use crate::state::GameState;
use shared::ClientRequest;

const BTN_PRIMARY: &str = "bg-orange-600 hover:bg-orange-500 text-white font-bold rounded-xl transition-all shadow-lg active:scale-95 disabled:opacity-50";

#[component]
pub fn MainMenu() -> impl IntoView {
    let state = use_context::<GameState>().expect("GameState missing");
    let (view_list, set_view_list) = create_signal(false);

    view! {
        <div class="flex flex-col items-center justify-center min-h-screen bg-slate-950 text-slate-200 p-4 font-sans">

            <Show when=move || state.connection_error.get().is_some()>
                <div class="mb-6 p-4 bg-red-900/30 border border-red-500/50 text-red-200 rounded-2xl text-sm max-w-md w-full animate-in fade-in duration-500">
                    <div class="flex items-center gap-3">
                        <span class="text-xl">"⚠️"</span>
                        <div>
                            <p class="font-bold">"Connection Error"</p>
                            <p class="text-xs opacity-80">{move || state.connection_error.get()}</p>
                        </div>
                    </div>
                </div>
            </Show>

            <Show when=move || state.is_connecting.get()>
                <div class="mb-6 flex items-center gap-2 text-orange-500 animate-pulse">
                    <div class="w-2 h-2 bg-orange-500 rounded-full"></div>
                    "Connecting to server..."
                </div>
            </Show>

x            <div class="p-10 bg-slate-900 rounded-3xl border border-slate-800 shadow-2xl text-center space-y-8 w-full max-w-md relative overflow-hidden">
                <div class="absolute -top-10 -right-10 w-32 h-32 bg-orange-600/10 rounded-full blur-3xl"></div>

                <h1 class="text-6xl font-black text-orange-600 italic tracking-tighter drop-shadow-sm">"COLONIST"</h1>

                <Show
                    when=move || view_list.get()
                    fallback=move || view! { <ActionButtons set_view_list=set_view_list state=state /> }
                >
                    <div class="space-y-4 animate-in slide-in-from-right duration-300">
                        <div class="flex justify-between items-center mb-2">
                            <h2 class="text-slate-400 font-bold uppercase text-xs tracking-widest">"Active Lobbies"</h2>
                            <button
                                on:click=move |_| set_view_list.set(false)
                                class="text-slate-500 hover:text-white text-xs underline transition-colors"
                            >
                                "Back"
                            </button>
                        </div>

                        <div class="max-h-64 overflow-y-auto space-y-2 pr-2 custom-scrollbar min-h-[100px]">
                            <For
                                each=move || state.lobby_games.get()
                                key=|g| g.0.clone()
                                children=move |lobby| view! { <LobbyItem lobby=lobby state=state /> }
                            />

                            <Show when=move || state.lobby_games.get().is_empty()>
                                <div class="py-10 text-slate-600 italic text-sm">"No games found..."</div>
                            </Show>
                        </div>

                        <button
                            on:click=move |_| state.send(ClientRequest::GetLobbyList)
                            class="text-xs text-orange-500/50 hover:text-orange-500 transition-colors flex items-center justify-center gap-1 w-full"
                        >
                            "🔄 Refresh list"
                        </button>
                    </div>
                </Show>
            </div>

            <p class="mt-8 text-slate-700 text-[10px] uppercase tracking-[0.3em]">"Powered by Rust & Leptos"</p>
        </div>
    }
}

#[component]
fn ActionButtons(set_view_list: WriteSignal<bool>, state: GameState) -> impl IntoView {
    view! {
        <div class="flex flex-col gap-4 animate-in zoom-in-95 duration-200">
            <button
                class=format!("{BTN_PRIMARY} py-4 text-xl")
                disabled=move || state.ws_sender.get().is_none()
                on:click=move |_| {
                    set_view_list.set(true);
                    state.send(ClientRequest::GetLobbyList);
                }
            >
                "BROWSE LOBBIES"
            </button>

            <button
                class="border-2 border-slate-700 hover:border-orange-600 text-slate-400 hover:text-white py-4 rounded-xl font-bold transition-all disabled:opacity-20"
                disabled=move || state.ws_sender.get().is_none()
                on:click=move |_| {
                    state.send(ClientRequest::CreateGame { player_count: 4 });
                }
            >
                "CREATE NEW GAME"
            </button>
        </div>
    }
}

#[component]
fn LobbyItem(lobby: (String, usize, usize), state: GameState) -> impl IntoView {
    let (id, current, max) = lobby;
    let is_full = current >= max;
    let gid = id.clone();

    view! {
        <div class="flex items-center justify-between p-4 bg-slate-950 border border-slate-800 rounded-2xl hover:border-orange-600/30 transition-all group">
            <div class="text-left">
                <div class="font-mono text-orange-500 font-bold text-sm">"ID: " {id}</div>
                <div class="text-[10px] text-slate-600 font-bold uppercase tracking-wider">{current} " / " {max} " Players"</div>
            </div>

            <button
                disabled=is_full || state.ws_sender.get().is_none()
                class=move || if is_full {
                    "bg-slate-800 text-slate-600 px-4 py-2 rounded-xl text-xs font-bold cursor-not-allowed".to_string()
                } else {
                    format!("{BTN_PRIMARY} px-4 py-2 text-xs")
                }
                on:click=move |_| {
                    state.send(ClientRequest::JoinGame { game_id: gid.clone() });
                }
            >
                {if is_full { "FULL" } else { "JOIN" }}
            </button>
        </div>
    }
}