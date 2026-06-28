use leptos::*;
use uuid::Uuid;
use crate::components::board::{player_color_hex, Board};
use crate::state::GameState;
use shared::ClientRequest;

#[component]
pub fn GamePage() -> impl IntoView {
    let state = use_context::<GameState>().expect("GameState missing");

    // Check if any modal is open - if so, disable pointer events on main UI
    let is_modal_open = move || {
        state.must_discard_count.get().is_some() ||
            !state.must_steal_from_players.get().is_empty() ||
            state.winner_player_id.get().is_some()
    };

    view! {
        <div class="h-screen flex flex-col overflow-hidden bg-slate-950 font-sans relative">
            // Discard cards modal
            <Show when=move || state.must_discard_count.get().is_some()>
                <DiscardCardsModal />
            </Show>

            // Rob player modal
            <Show when=move || !state.must_steal_from_players.get().is_empty()>
                <RobPlayerModal />
            </Show>

            // Year of Plenty modal
            <Show when=move || state.year_of_plenty_pending.get()>
                <YearOfPlentyModal />
            </Show>

            // Monopoly modal
            <Show when=move || state.monopoly_pending.get()>
                <MonopolyModal />
            </Show>

            // Main game UI - disable pointer events when modal is open
            <div class=move || if is_modal_open() { "flex-1 flex flex-col min-h-0 pointer-events-none" } else { "flex-1 flex flex-col min-h-0 pointer-events-auto" }>
            <header class="flex justify-between items-center bg-slate-900/50 border-b border-slate-800 p-4 backdrop-blur-md z-10">
                <div class="flex items-center gap-6">
                    <div>
                        <h1 class="text-2xl font-black italic text-orange-600 tracking-tighter leading-none">"COLONIST"</h1>
                        <span class="text-[9px] text-slate-500 font-mono tracking-widest uppercase">"Live Session"</span>
                    </div>

                    <div class="hidden md:flex items-center gap-4 bg-black/30 px-4 py-2 rounded-lg border border-slate-800">
                        {move || {
                            let resources = state.my_resources.get();
                            view! {
                                <ResourceIcon label="Brick" color="text-red-500" count=resources.brick as i32 />
                                <ResourceIcon label="Wood" color="text-green-500" count=resources.lumber as i32 />
                                <ResourceIcon label="Sheep" color="text-lime-400" count=resources.wool as i32 />
                                <ResourceIcon label="Wheat" color="text-yellow-400" count=resources.grain as i32 />
                                <ResourceIcon label="Ore" color="text-slate-400" count=resources.ore as i32 />
                            }
                        }}
                    </div>
                </div>
                <div class="flex items-center gap-4">
                    <button
                    class="bg-red-900/40 hover:bg-red-700 text-red-200 px-4 py-2 rounded-lg font-bold transition-all border border-red-800/50 active:scale-95 text-xs"
                    on:click=move |_| {
                        if let Some(game_id) = state.game_id.get() {
                            state.send(ClientRequest::LeaveGame { game_id });
                            //let _ = window().location().set_href("/");
                        }
                    }
                        >
                    "LEAVE GAME"
                    </button>
                    // Debug button - always visible, disabled when not applicable
                    <button
                        class="bg-purple-600 hover:bg-purple-500 text-white px-4 py-2 rounded-lg font-bold transition-all shadow-lg active:scale-95 text-xs border-2 border-purple-400 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:bg-purple-600"
                        disabled=move || {
                            logging::log!("Checking if Skip Setup button should be disabled...");
                            logging::log!("Current game phase: {:?}", state.game_phase.get());
                            let phase = move || state.game_phase.get();
                            let ws_ready = state.ws_sender.get().is_some();
                            !ws_ready || !(phase() == shared::GamePhase::InitialPlacementRound1 ||
                                           phase() == shared::GamePhase::InitialPlacementRound2)
                        }
                        on:click=move |_| {
                            logging::log!("Skip setup button clicked");
                            state.send(ClientRequest::DebugAutoInitialPlacement);
                        }
                        title="Skip initial placement phase (debug)"
                    >
                        "⚡ SKIP SETUP"
                    </button>

                    <button
                        class="bg-orange-600 hover:bg-orange-500 text-white px-6 py-2 rounded-lg font-bold transition-all shadow-lg active:scale-95 text-sm"
                        on:click=move |_| state.send(ClientRequest::EndTurn)
                    >
                        "END TURN"
                    </button>
                </div>
            </header>

            <div class="flex-1 flex overflow-hidden min-h-0">
                <aside  class="w-64 bg-slate-900/30 border-r border-slate-800
                                flex flex-col flex-shrink-0 overflow-hidden">
                    <div class="p-4 flex-1 overflow-y-auto min-h-0 custom-scrollbar space-y-6 pb-10">
                        <div>
                            <h3 class="text-slate-500 font-bold text-[10px] uppercase tracking-[0.2em] mb-3">"Players"</h3>
                            <div class="space-y-2">
                                <For
                                    each=move || state.players.get()
                                    key=|player| player.player_id
                                    children=move |player| {
                                        let is_me = Some(player.player_id) == state.player_id.get();
                                        let player_clone = player.clone();
                                        let is_active = move || player_clone.player_id == state.current_turn_player.get();

                                        let score_signal = create_read_slice(
                                        state.players,
                                        move |players| {
                                            players.iter()
                                                .find(|p| p.player_id == player.player_id)
                                                .map(|p| (p.victory_points as i32, state.secret_victory_points.get() ))
                                                .unwrap_or((0, 0))
                                        }
                                        );

                                        let player_id = player.player_id;
                                        let resource_count = create_read_slice(
                                            state.players,
                                            move |players| {
                                                players.iter()
                                                    .find(|p| p.player_id == player_id)
                                                    .map(|p| {
                                                        let r = &p.resources;
                                                        (r.brick + r.lumber + r.wool + r.grain + r.ore) as i32
                                                    })
                                                    .unwrap_or(0)
                                            }
                                        );

                                        let dev_card_count = create_read_slice(
                                            state.players,
                                            move |players| {
                                                players.iter()
                                                    .find(|p| p.player_id == player_id)
                                                    .map(|p| p.dev_cards.len() as i32)
                                                    .unwrap_or(0)
                                            }
                                        );

                                        let knights_played = create_read_slice(
                                            state.players,
                                            move |players| {
                                                players.iter()
                                                    .find(|p| p.player_id == player_id)
                                                    .map(|p| p.knights_played as i32)
                                                    .unwrap_or(0)
                                            }
                                        );

                                        let roads_count = create_read_slice(
                                            state.players,
                                            move |players| {
                                                players.iter()
                                                    .find(|p| p.player_id == player_id)
                                                    .map(|p| p.roads_count as i32)
                                                    .unwrap_or(0)
                                            }
                                        );

                                        let has_longest_road = create_read_slice(
                                            state.players,
                                            move |players| {
                                                players.iter()
                                                    .find(|p| p.player_id == player_id)
                                                    .map(|p| p.has_longest_road)
                                                    .unwrap_or(false)
                                            }
                                        );

                                        let has_largest_army = create_read_slice(
                                            state.players,
                                            move |players| {
                                                players.iter()
                                                    .find(|p| p.player_id == player_id)
                                                    .map(|p| p.has_largest_army)
                                                    .unwrap_or(false)
                                            }
                                        );

                                        let display_name = if is_me {
                                            format!("{} (You)", player.name)
                                        } else {
                                            player.name.clone()
                                        };

                                        view! {
                                            <PlayerTagDynamic
                                                name=display_name
                                                score=score_signal
                                                is_active=is_active
                                                color=player.color.clone()
                                                is_me=player.player_id == state.player_id.get().unwrap_or(Uuid::nil())
                                                resource_count=resource_count
                                                dev_card_count=dev_card_count
                                                knights_played=knights_played
                                                roads_count=roads_count
                                                has_longest_road=has_longest_road
                                                has_largest_army=has_largest_army
                                            />
                                        }
                                    }
                                />
                            </div>
                        </div>

                        <div class="space-y-4">
                            <div>
                                <h3 class="text-slate-500 font-bold text-[10px] uppercase tracking-[0.2em] mb-3">"Bank Trading"</h3>
                                <div class="text-xs text-slate-400 mb-2">"Trade resources with the bank"</div>
                                <Show when=move || {
                                    let is_my_turn = state.player_id.get().map(|id| id == state.current_turn_player.get()).unwrap_or(false);
                                    let has_rolled = state.last_dice_roll.get().is_some();
                                    is_my_turn && has_rolled
                                }>
                                    <BankTradeUI />
                                </Show>
                            </div>

                            <div>
                                <h3 class="text-slate-500 font-bold text-[10px] uppercase tracking-[0.2em] mb-3">"Player Trading"</h3>
                                <PlayerTradeUI />
                            </div>

                            <div>
                                <h3 class="text-slate-500 font-bold text-[10px] uppercase tracking-[0.2em] mb-3">"Development"</h3>
                                <button
                                    class="w-full py-2 bg-slate-800 hover:bg-slate-700 border border-slate-700 rounded-lg text-xs font-bold transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                                    on:click=move |_| {
                                        state.send(ClientRequest::BuyDevelopmentCard);
                                    }
                                    disabled=move || {
                                        // Can only buy if it's your turn and you've rolled
                                        let is_my_turn = state.player_id.get().map(|id| id == state.current_turn_player.get()).unwrap_or(false);
                                        let has_rolled = state.last_dice_roll.get().is_some();
                                        let phase = move || state.game_phase.get();
                                        !(is_my_turn && has_rolled && phase() == shared::GamePhase::RegularPlay)
                                    }
                                >
                                    "BUY CARD"
                                </button>

                                <div class="mt-3 space-y-1">
                                    <div class="text-[10px] text-slate-400 mb-1">"Your Cards:"</div>
                                    {move || {
                                        let cards = state.my_dev_cards.get();
                                        if cards.is_empty() {
                                            view! {
                                                <div class="text-[10px] text-slate-600 italic py-2">"No cards yet"</div>
                                            }.into_view()
                                        } else {
                                            cards.into_iter().map(|card| {
                                                let card_clone = card.clone();
                                                let card_name = match card {
                                                    shared::DevCardType::Knight => "⚔️ Knight",
                                                    shared::DevCardType::VictoryPoint => "🏆 Victory Point",
                                                    shared::DevCardType::RoadBuilding => "🛣️ Road Building",
                                                    shared::DevCardType::Monopoly => "💰 Monopoly",
                                                    shared::DevCardType::YearOfPlenty => "🌾 Year of Plenty",
                                                };

                                                view! {
                                                    <button
                                                        class="w-full py-1.5 px-2 bg-purple-900/30 hover:bg-purple-800/40 border border-purple-700/50 rounded text-[10px] font-bold transition-colors disabled:opacity-50 disabled:cursor-not-allowed text-left"
                                                        on:click=move |_| {
                                                            state.send(ClientRequest::PlayDevCard { card: card_clone.clone(), target: None });
                                                        }
                                                    >
                                                        {card_name}
                                                    </button>
                                                }
                                            }).collect_view()
                                        }
                                    }}
                                </div>
                            </div>
                        </div>
                    </div>
                </aside>

                <div class="flex-1 flex items-start justify-center overflow-y-auto overflow-x-hidden min-h-0 min-w-0 py-4">
                    <Board />
                </div>

                <aside class="w-72 bg-slate-900/30 border-l border-slate-800 flex flex-col overflow-hidden shrink-0">
                    <div class="p-4 border-b border-slate-800">
                        <h3 class="text-slate-500 font-bold text-[10px] uppercase tracking-[0.2em]">"Event Log"</h3>
                    </div>
                    <div class="flex-1 overflow-y-auto p-4 space-y-2 custom-scrollbar">
                        <For
                            each=move || state.messages.get().into_iter().rev()
                            key=|msg| msg.clone()
                            children=move |msg| view! {
                                <div class="text-[11px] font-mono text-slate-400 border-l-2 border-slate-700 pl-2 py-1 bg-slate-800/20">
                                    {msg}
                                </div>
                            }
                        />
                        <Show when=move || state.messages.get().is_empty()>
                            <div class="text-xs text-slate-600 italic">"Waiting for actions..."</div>
                        </Show>
                    </div>
                </aside>
            </div>

            // Bottom status bar - show when waiting for discards (even if I'm also discarding)
            <Show when=move || state.waiting_for_discards.get() && !state.must_move_robber.get()>
                <div class="fixed bottom-0 left-0 right-0 z-[9998] bg-yellow-900/80 border-t border-yellow-600 px-4 py-3 flex items-center justify-center gap-3 backdrop-blur-sm">
                    <div class="animate-pulse w-3 h-3 bg-yellow-400 rounded-full"></div>
                    <span class="text-yellow-200 font-bold text-sm">"Waiting for other players to discard cards..."</span>
                    <div class="animate-pulse w-3 h-3 bg-yellow-400 rounded-full"></div>
                </div>
            </Show>
            <Show when=move || state.winner_player_id.get().is_some()>
                    <div class="fixed inset-0 z-[10000] flex items-center justify-center bg-black/60 backdrop-blur-md pointer-events-auto">
                        <div class="bg-slate-900 border-4 border-yellow-500 rounded-3xl p-10 flex flex-col items-center shadow-[0_0_50px_rgba(234,179,8,0.3)] animate-in fade-in zoom-in duration-300">
                            <div class="text-7xl mb-4">"🏆"</div>
                            <h2 class="text-5xl font-black text-white mb-2 tracking-tighter">"VICTORY"</h2>
                            <div class="h-1 w-32 bg-yellow-500 mb-6"></div>

                            <p class="text-2xl text-slate-300 mb-8 text-center">
                                {move || {
                                    let winner_id = state.winner_player_id.get().unwrap_or_default();
                                    state.players.get().iter()
                                        .find(|p| p.player_id == winner_id)
                                        .map(|p| p.name.clone())
                                        .unwrap_or_else(|| "Unknown Explorer".to_string())
                                }}
                                <span class="block text-yellow-500 font-bold mt-2">"has colonized the island!"</span>
                            </p>

                            <button
                                class="bg-yellow-600 hover:bg-yellow-500 text-white px-10 py-4 rounded-xl font-black transition-all shadow-lg active:scale-95"
                                on:click=|_| {
                                    // Logic to return to lobby or refresh
                                    let _ = window().location().set_href("/");
                                }
                            >
                                "RETURN TO LOBBY"
                            </button>
                        </div>
                    </div>
                </Show>
            </div> // Close pointer-events wrapper
        </div>
    }
}

#[component]
fn ResourceIcon(label: &'static str, color: &'static str, count: i32) -> impl IntoView {
    view! {
        <div class="flex flex-col items-center">
            <span class=format!("text-[10px] font-bold uppercase {color}")>{label}</span>
            <span class="text-lg font-black leading-none">{count}</span>
        </div>
    }
}

#[component]
fn PlayerTag(name: &'static str, score: i32, is_active: bool) -> impl IntoView {
    let active_class = if is_active { "border-orange-600 bg-orange-600/10" } else { "border-slate-800 bg-transparent" };
    view! {
        <div class=format!("flex justify-between items-center p-3 border rounded-xl transition-all {active_class}")>
            <div class="flex items-center gap-2">
                <div class=format!("w-2 h-2 rounded-full {}", if is_active { "bg-orange-500 animate-pulse" } else { "bg-slate-600" })></div>
                <span class="text-sm font-bold">{name}</span>
            </div>
            <span class="bg-black/40 px-2 py-1 rounded text-[10px] font-mono font-bold text-orange-400">{score} " VP"</span>
        </div>
    }
}

#[component]
fn PlayerTagDynamic(
    name: String,
    score: Signal<(i32, i32)>,
    is_active: impl Fn() -> bool + 'static + Clone,
    color: String,
    is_me: bool,
    resource_count: Signal<i32>,
    dev_card_count: Signal<i32>,
    knights_played: Signal<i32>,
    roads_count: Signal<i32>,
    has_longest_road: Signal<bool>,
    has_largest_army: Signal<bool>
) -> impl IntoView {
    let hex_color = player_color_hex(&color);
    let is_active_for_style = is_active.clone();
    let is_active_for_class = is_active.clone();
    let active_class = move || {
        if is_active_for_class() {
            "border-orange-600 bg-orange-600/10"
        } else {
            "border-slate-800"
        }
    };
    let inactive_style = move || {
        if !is_active_for_style() {
            format!("background-color: {}33;", hex_color)
        } else {
            "".to_string()
        }
    };
    view! {
        <div
            class=move || format!("flex flex-col p-3 border rounded-xl transition-all {}", active_class())
            style=inactive_style
        >
            <div class="flex justify-between items-center">
                <div class="flex items-center gap-2 min-w-0">
                    <div
                        class="w-2.5 h-2.5 rounded-full border border-black/40 shrink-0"
                        style=move || format!("background-color: {};", hex_color)
                    ></div>
                    <span class="text-sm font-bold truncate" style=move || format!("color: {};", hex_color)>{name}</span>
                    <Show when=is_active>
                        <span class="bg-orange-600 text-white px-2 py-1 rounded-full text-[9px] font-bold">"TURN"</span>
                    </Show>
                    <Show when=move || has_longest_road.get()>
                        <span class="bg-yellow-700 text-white px-2 py-1 rounded-full text-[9px] font-bold" title="Longest Road (2 VP)">"🛣️"</span>
                    </Show>
                    <Show when=move || has_largest_army.get()>
                        <span class="bg-red-700 text-white px-2 py-1 rounded-full text-[9px] font-bold" title="Largest Army (2 VP)">"⚔️"</span>
                    </Show>
                </div>
                <span class="bg-black/40 px-2 py-1 rounded text-[10px] font-mono font-bold text-orange-400">
                    {move || {
                        let (vp, secret) = score.get();
                        if is_me{
                            format!("{vp} VP + ({secret})")
                        } else {
                            format!("{vp} VP")
                        }
                    }}
                </span>
            </div>
            <div class="flex gap-3 mt-2 text-[10px] text-slate-400">
                <span>
                    "🃏 " {move || resource_count.get()} " resources"
                </span>
                <span>
                    "🎴 " {move || dev_card_count.get()} " dev cards"
                </span>
                <span>
                    "⚔️ " {move || knights_played.get()} " knights"
                </span>
                <span>
                    "🛣️ " {move || roads_count.get()} " road length"
                </span>
            </div>
        </div>
    }
}

#[component]
fn DiscardCardsModal() -> impl IntoView {
    let state = use_context::<GameState>().expect("GameState missing");

    let discard_count = move || state.must_discard_count.get().unwrap_or(0);

    // Track how many of each resource to discard
    let (discard_brick, set_discard_brick) = create_signal(0u8);
    let (discard_lumber, set_discard_lumber) = create_signal(0u8);
    let (discard_wool, set_discard_wool) = create_signal(0u8);
    let (discard_grain, set_discard_grain) = create_signal(0u8);
    let (discard_ore, set_discard_ore) = create_signal(0u8);

    // Log resources reactively
    create_effect(move |_| {
        let my_res = state.my_resources.get();
        logging::log!("Discard modal - Resources: Brick={}, Lumber={}, Wool={}, Grain={}, Ore={}",
            my_res.brick, my_res.lumber, my_res.wool, my_res.grain, my_res.ore);
        logging::log!("Must discard: {} cards", discard_count());
    });

    // Reset selections when modal closes
    create_effect(move |_| {
        if state.must_discard_count.get().is_none() {
            set_discard_brick.set(0);
            set_discard_lumber.set(0);
            set_discard_wool.set(0);
            set_discard_grain.set(0);
            set_discard_ore.set(0);
        }
    });

    // Timer: 30 seconds
    let (time_left, set_time_left) = create_signal(30);

    let total_discarded = move || {
        discard_brick.get() + discard_lumber.get() + discard_wool.get() + discard_grain.get() + discard_ore.get()
    };

    let can_confirm = move || total_discarded() == discard_count() as u8;

    let confirm_discard = move |_| {
        if can_confirm() {
            logging::log!("📤 Sending DiscardCards request to server...");
            state.send(ClientRequest::DiscardCards {
                resources: shared::Resources {
                    brick: discard_brick.get(),
                    lumber: discard_lumber.get(),
                    wool: discard_wool.get(),
                    grain: discard_grain.get(),
                    ore: discard_ore.get(),
                }
            });
            // Don't close modal yet - wait for ServerMessage::CardsDiscarded confirmation
            // Modal will be closed in state.rs when we receive CardsDiscarded
            logging::log!("⏳ Waiting for server confirmation...");
            // Reset counters
            set_discard_brick.set(0);
            set_discard_lumber.set(0);
            set_discard_wool.set(0);
            set_discard_grain.set(0);
            set_discard_ore.set(0);
        }
    };

    // Start countdown timer and handle auto-discard
    create_effect(move |prev_handle: Option<Option<leptos_dom::helpers::IntervalHandle>>| {
        // Clean up previous interval if it exists
        if let Some(Some(handle)) = prev_handle {
            handle.clear();
            logging::log!("🧹 Cleared previous timer interval");
        }

        if state.must_discard_count.get().is_some() {
            // Reset timer when modal opens
            set_time_left.set(30);
            logging::log!("⏱️ Starting 30 second discard timer");

            let state_timer = state;
            match set_interval_with_handle(
                move || {
                    // Check if modal is still open
                    if state_timer.must_discard_count.get_untracked().is_none() {
                        // Modal closed, just return without updating signals
                        return;
                    }

                    let current = time_left.get_untracked();
                    if current > 0 {
                        set_time_left.set(current - 1);
                        if current % 5 == 0 {
                            logging::log!("⏱️ Timer: {} seconds remaining", current);
                        }
                    } else {
                        // Time's up! Auto-discard random cards
                        logging::log!("⏰ Timer expired! Auto-discarding cards...");
                        logging::log!("Player ID: {:?}", state_timer.player_id.get_untracked());

                        let my_res = state_timer.my_resources.get_untracked();
                        let target = state_timer.must_discard_count.get_untracked().unwrap_or(0) as u8;

                        logging::log!("Discard target: {} cards", target);
                        logging::log!("My resources: Brick={}, Lumber={}, Wool={}, Grain={}, Ore={}",
                            my_res.brick, my_res.lumber, my_res.wool, my_res.grain, my_res.ore);

                        if target == 0 {
                            logging::log!("❌ No cards to discard, closing modal");
                            state_timer.must_discard_count.set(None);
                            return;
                        }

                        logging::log!("Target: {} cards, Current resources: Brick={}, Lumber={}, Wool={}, Grain={}, Ore={}",
                            target, my_res.brick, my_res.lumber, my_res.wool, my_res.grain, my_res.ore);

                        // Randomly select resources to discard
                        let mut resources = vec![
                            (my_res.brick, "brick"),
                            (my_res.lumber, "lumber"),
                            (my_res.wool, "wool"),
                            (my_res.grain, "grain"),
                            (my_res.ore, "ore"),
                        ];

                        // Filter out resources we don't have
                        resources.retain(|(count, _)| *count > 0);
                        logging::log!("Available resources to discard from: {} types", resources.len());

                        if resources.is_empty() {
                            logging::log!("❌ No resources available to discard!");
                            state_timer.must_discard_count.set(None);
                            return;
                        }

                        let mut discarded = 0u8;
                        let mut brick_d = 0u8;
                        let mut lumber_d = 0u8;
                        let mut wool_d = 0u8;
                        let mut grain_d = 0u8;
                        let mut ore_d = 0u8;

                        // Keep discarding until we reach the target
                        while discarded < target && !resources.is_empty() {
                            // Pick a random resource
                            let idx = (js_sys::Math::random() * resources.len() as f64).floor() as usize;
                            let (_, name) = resources[idx];

                            match name {
                                "brick" => { brick_d += 1; resources[idx].0 -= 1; }
                                "lumber" => { lumber_d += 1; resources[idx].0 -= 1; }
                                "wool" => { wool_d += 1; resources[idx].0 -= 1; }
                                "grain" => { grain_d += 1; resources[idx].0 -= 1; }
                                "ore" => { ore_d += 1; resources[idx].0 -= 1; }
                                _ => {}
                            }

                            if resources[idx].0 == 0 {
                                resources.remove(idx);
                            }
                            discarded += 1;
                        }

                        logging::log!("✅ Auto-discarding: Brick={}, Lumber={}, Wool={}, Grain={}, Ore={}",
                            brick_d, lumber_d, wool_d, grain_d, ore_d);

                        state_timer.send(ClientRequest::DiscardCards {
                            resources: shared::Resources {
                                brick: brick_d,
                                lumber: lumber_d,
                                wool: wool_d,
                                grain: grain_d,
                                ore: ore_d,
                            }
                        });
                        state_timer.must_discard_count.set(None);
                    }
                },
                std::time::Duration::from_secs(1),
            ) {
                Ok(handle) => Some(handle),
                Err(_) => {
                    logging::error!("Failed to create interval");
                    None
                }
            }
        } else {
            logging::log!("Modal closed, no timer needed");
            None
        }
    });

    // Helper to create discard counter row - closures with >= are defined outside view! macro
    // to avoid Leptos parsing issues with >= being interpreted as HTML tag closing
    let discard_counter = move |name: &'static str, color: &'static str,
                                 value: ReadSignal<u8>, setter: WriteSignal<u8>,
                                 get_max: Signal<u8>| {
        // Define closures outside the view! macro to avoid parsing issues with >=
        let dec_disabled = move || value.get() == 0;
        let inc_disabled = move || value.get() >= get_max.get();
        let on_dec = move |_| {
            if value.get() > 0 {
                setter.set(value.get() - 1);
            }
        };
        let on_inc = move |_| {
            if value.get() < get_max.get() {
                setter.set(value.get() + 1);
            }
        };

        view! {
            <div class="flex items-center justify-between bg-slate-800/50 p-3 rounded-lg">
                <div class="flex items-center gap-3">
                    <span class=format!("font-bold {}", color)>{name}</span>
                    <span class="text-xs text-slate-500">"(have " {move || get_max.get()} ")"</span>
                </div>
                <div class="flex items-center gap-2">
                    <button
                        class="w-8 h-8 bg-red-700 hover:bg-red-600 rounded font-bold text-white disabled:opacity-30 disabled:cursor-not-allowed text-lg"
                        on:click=on_dec
                        disabled=dec_disabled
                    >
                        "-"
                    </button>
                    <span class="w-12 text-center font-bold text-white text-lg">{value}</span>
                    <button
                        class="w-8 h-8 bg-green-700 hover:bg-green-600 rounded font-bold text-white disabled:opacity-30 disabled:cursor-not-allowed text-lg"
                        on:click=on_inc
                        disabled=inc_disabled
                    >
                        "+"
                    </button>
                </div>
            </div>
        }
    };

    // Create signals for max values (from player's resources)
    let max_brick = Signal::derive(move || state.my_resources.get().brick);
    let max_lumber = Signal::derive(move || state.my_resources.get().lumber);
    let max_wool = Signal::derive(move || state.my_resources.get().wool);
    let max_grain = Signal::derive(move || state.my_resources.get().grain);
    let max_ore = Signal::derive(move || state.my_resources.get().ore);

    view! {
        <div class="fixed inset-0 bg-black/80 flex items-center justify-center z-[9999] backdrop-blur-sm pointer-events-auto">
            <div class="bg-slate-900 border-2 border-red-600 rounded-2xl p-6 max-w-md w-full mx-4 shadow-2xl">
                <div class="flex justify-between items-center mb-2">
                    <h2 class="text-2xl font-bold text-red-500">"⚠️ Discard Cards"</h2>
                    <div class=move || {
                        let t = time_left.get();
                        if t <= 10 {
                            "text-2xl font-black text-red-500 animate-pulse"
                        } else {
                            "text-2xl font-black text-yellow-500"
                        }
                    }>
                        {time_left} "s"
                    </div>
                </div>
                <p class="text-slate-300 mb-4">
                    "You must discard " <span class="text-red-400 font-bold">{discard_count}</span> " cards. Select which resources to discard:"
                </p>

                <div class="space-y-3 mb-4">
                    {discard_counter("Brick", "text-red-500", discard_brick, set_discard_brick, max_brick)}
                    {discard_counter("Wood", "text-green-500", discard_lumber, set_discard_lumber, max_lumber)}
                    {discard_counter("Sheep", "text-lime-400", discard_wool, set_discard_wool, max_wool)}
                    {discard_counter("Wheat", "text-yellow-400", discard_grain, set_discard_grain, max_grain)}

                    {discard_counter("Ore", "text-slate-400", discard_ore, set_discard_ore, max_ore)}
                </div>

                <div class="flex justify-between items-center pt-4 border-t border-slate-700">
                    <div class="text-sm text-slate-400">
                        "Selected: " <span class=move || if can_confirm() { "text-green-400 font-bold" } else { "text-red-400 font-bold" }>{total_discarded} " / " {discard_count}</span>
                    </div>
                    <button
                        class="bg-red-600 hover:bg-red-500 text-white px-6 py-2 rounded-lg font-bold transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                        on:click=confirm_discard
                        disabled=move || !can_confirm()
                    >
                        "DISCARD"
                    </button>
                </div>
            </div>
        </div>
    }
}

#[component]
fn RobPlayerModal() -> impl IntoView {
    let state = use_context::<GameState>().expect("GameState missing");

    let rob_player = move |victim_id: Uuid| {
        state.send(ClientRequest::StealFromPlayer { victim_id });
        state.must_steal_from_players.set(Vec::new());
    };

    view! {
        <div class="fixed inset-0 bg-black/80 flex items-center justify-center z-[9999] backdrop-blur-sm pointer-events-auto">
            <div class="bg-slate-900 border-2 border-orange-600 rounded-2xl p-6 max-w-md w-full mx-4 shadow-2xl">
                <h2 class="text-2xl font-bold text-orange-500 mb-2">"🦹 Rob a Player"</h2>
                <p class="text-slate-300 mb-4">
                    "Select a player to steal a random resource card from:"
                </p>

                <div class="space-y-2 mb-4">
                    <For
                        each=move || state.must_steal_from_players.get()
                        key=|id| *id
                        children=move |victim_id| {
                            let player_name = state.players.get()
                                .iter()
                                .find(|p| p.player_id == victim_id)
                                .map(|p| p.name.clone())
                                .unwrap_or_else(|| format!("Player {}", victim_id));

                            view! {
                                <button
                                    class="w-full py-3 px-4 bg-slate-800 hover:bg-orange-600 border border-slate-700 rounded-lg text-sm font-bold transition-colors text-left"
                                    on:click=move |_| rob_player(victim_id)
                                >
                                    {player_name}
                                </button>
                            }
                        }
                    />
                </div>
            </div>
        </div>
    }
}

#[component]
fn BankTradeUI() -> impl IntoView {
    let state = use_context::<GameState>().expect("GameState missing");

    // Track selected resources for trading
    let (give_resource, set_give_resource) = create_signal::<Option<shared::ResourceType>>(None);
    let (receive_resource, set_receive_resource) = create_signal::<Option<shared::ResourceType>>(None);

    // Get ratio for a resource based on player's ports
    let get_ratio = move |res: shared::ResourceType| -> u8 {
        state.get_best_ratio(res)
    };

    let get_resource_count = move |res: shared::ResourceType| {
        let r = state.my_resources.get();
        match res {
            shared::ResourceType::Brick => r.brick,
            shared::ResourceType::Wood => r.lumber,
            shared::ResourceType::Wool | shared::ResourceType::Sheep => r.wool,
            shared::ResourceType::Wheat => r.grain,
            shared::ResourceType::Ore => r.ore,
            _ => 0,
        }
    };
    // Check if player can afford to trade a resource (using correct ratio)
    let can_give = move |res: shared::ResourceType| -> bool {
        let ratio = get_ratio(res);
        let count = get_resource_count(res);
        count >= ratio
    };

    // Check if trade is valid (give and receive selected, different resources, can afford)
    let can_trade = move || {
        if let (Some(give), Some(receive)) = (give_resource.get(), receive_resource.get()) {
            give != receive && can_give(give)
        } else {
            false
        }
    };

    let execute_trade = move |_| {
        if let (Some(give), Some(receive)) = (give_resource.get(), receive_resource.get()) {
            state.send(ClientRequest::BankTrade { give, receive });
            // Reset selections after trade
            set_give_resource.set(None);
            set_receive_resource.set(None);
        }
    };

    // Resource button with ratio display
    let resource_btn = move |res: shared::ResourceType, name: &'static str, is_give: bool,
                              selected: Option<shared::ResourceType>,
                              set_resource: WriteSignal<Option<shared::ResourceType>>| {
        let is_selected = selected == Some(res);

        let base = "px-1 py-1 rounded text-[9px] font-bold transition-all border flex flex-col items-center";

        view! {
            <button
                class=move || {
                    let can_afford = can_give(res);
                    if is_give && !can_afford {
                        format!("{} bg-slate-800/30 text-slate-600 border-slate-700 cursor-not-allowed", base)
                    } else if is_selected {
                        format!("{} bg-orange-600 text-white border-orange-500", base)
                    } else {
                        format!("{} bg-slate-800 hover:bg-slate-700 text-slate-300 border-slate-600", base)
                    }
                }
                disabled=move || is_give && !can_give(res)
                on:click=move |_| set_resource.set(Some(res))
            >
                <span>{name}</span>
                {if is_give {
                    view! { <span class=move || {
                        let ratio = get_ratio(res);
                        if ratio == 2 {
                            "text-green-400"
                        } else if ratio == 3 {
                            "text-yellow-400"
                        } else {
                            "text-slate-500"
                        }
                    }>{move || format!("{}:1", get_ratio(res))}</span> }.into_view()
                } else {
                    view! { <span></span> }.into_view()
                }}
            </button>
        }
    };

    // Calculate best overall ratio for display
    let best_ratio = move || {
        let mut best = 4u8;
        for res in [shared::ResourceType::Brick, shared::ResourceType::Wood,
                    shared::ResourceType::Sheep, shared::ResourceType::Wheat, shared::ResourceType::Ore] {
            best = best.min(get_ratio(res));
        }
        best
    };

    view! {
        <div class="space-y-2">
            // Show port bonus indicator if player has any
            <Show when=move || best_ratio() < 4>
                <div class="text-[9px] text-green-400 bg-green-900/30 px-2 py-1 rounded border border-green-700/50">
                    {move || {
                        let ports = state.my_ports.get();
                        let has_3to1 = ports.iter().any(|p| matches!(p, shared::PortType::ThreeToOne));
                        let two_to_one: Vec<_> = ports.iter().filter_map(|p| {
                            if let shared::PortType::TwoToOne(res) = p {
                                Some(match res {
                                    shared::ResourceType::Brick => "Brick",
                                    shared::ResourceType::Wood => "Wood",
                                    shared::ResourceType::Wool | shared::ResourceType::Sheep => "Sheep",
                                    shared::ResourceType::Wheat => "Wheat",
                                    shared::ResourceType::Ore => "Ore",
                                    _ => "?",
                                })
                            } else { None }
                        }).collect();

                        let mut parts = Vec::new();
                        if has_3to1 { parts.push("3:1 any".to_string()); }
                        for r in two_to_one { parts.push(format!("2:1 {}", r)); }
                        format!("Ports: {}", parts.join(", "))
                    }}
                </div>
            </Show>

            // Give section
            <div>
                <div class="text-[10px] text-slate-500 mb-1">"Give:"</div>
                <div class="flex flex-wrap gap-1">
                    {move || resource_btn(shared::ResourceType::Brick, "Brick", true, give_resource.get(), set_give_resource)}
                    {move || resource_btn(shared::ResourceType::Wood, "Wood", true, give_resource.get(), set_give_resource)}
                    {move || resource_btn(shared::ResourceType::Sheep, "Sheep", true, give_resource.get(), set_give_resource)}
                    {move || resource_btn(shared::ResourceType::Wheat, "Wheat", true, give_resource.get(), set_give_resource)}
                    {move || resource_btn(shared::ResourceType::Ore, "Ore", true, give_resource.get(), set_give_resource)}
                </div>
            </div>

            // Receive section
            <div>
                <div class="text-[10px] text-slate-500 mb-1">"Receive 1:"</div>
                <div class="flex flex-wrap gap-1">
                    {move || resource_btn(shared::ResourceType::Brick, "Brick", false, receive_resource.get(), set_receive_resource)}
                    {move || resource_btn(shared::ResourceType::Wood, "Wood", false, receive_resource.get(), set_receive_resource)}
                    {move || resource_btn(shared::ResourceType::Sheep, "Sheep", false, receive_resource.get(), set_receive_resource)}
                    {move || resource_btn(shared::ResourceType::Wheat, "Wheat", false, receive_resource.get(), set_receive_resource)}
                    {move || resource_btn(shared::ResourceType::Ore, "Ore", false, receive_resource.get(), set_receive_resource)}
                </div>
            </div>

            // Trade button with dynamic ratio
            <button
                class="w-full py-2 bg-green-700 hover:bg-green-600 rounded text-[11px] font-bold transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                disabled=move || !can_trade()
                on:click=execute_trade
            >
                {move || {
                    if let Some(give) = give_resource.get() {
                        format!("TRADE {} for 1", get_ratio(give))
                    } else {
                        "TRADE".to_string()
                    }
                }}
            </button>
        </div>
    }
}

#[component]
fn PlayerTradeUI() -> impl IntoView {
    let state = use_context::<GameState>().expect("GameState missing");

    // Trade offer form state
    let (show_form, set_show_form) = create_signal(false);
    let (offer_brick, set_offer_brick) = create_signal(0u8);
    let (offer_lumber, set_offer_lumber) = create_signal(0u8);
    let (offer_wool, set_offer_wool) = create_signal(0u8);
    let (offer_grain, set_offer_grain) = create_signal(0u8);
    let (offer_ore, set_offer_ore) = create_signal(0u8);
    let (request_brick, set_request_brick) = create_signal(0u8);
    let (request_lumber, set_request_lumber) = create_signal(0u8);
    let (request_wool, set_request_wool) = create_signal(0u8);
    let (request_grain, set_request_grain) = create_signal(0u8);
    let (request_ore, set_request_ore) = create_signal(0u8);

    // Check if player can afford the offered resources
    let can_afford_offer = move || {
        let res = state.my_resources.get();
        offer_brick.get() <= res.brick &&
        offer_lumber.get() <= res.lumber &&
        offer_wool.get() <= res.wool &&
        offer_grain.get() <= res.grain &&
        offer_ore.get() <= res.ore
    };

    // Check if trade offer is valid (at least offering or requesting something)
    let is_valid_offer = move || {
        let offering_something = offer_brick.get() > 0 || offer_lumber.get() > 0 ||
            offer_wool.get() > 0 || offer_grain.get() > 0 || offer_ore.get() > 0;
        let requesting_something = request_brick.get() > 0 || request_lumber.get() > 0 ||
            request_wool.get() > 0 || request_grain.get() > 0 || request_ore.get() > 0;
        offering_something && requesting_something && can_afford_offer()
    };

    let reset_form = move || {
        set_offer_brick.set(0);
        set_offer_lumber.set(0);
        set_offer_wool.set(0);
        set_offer_grain.set(0);
        set_offer_ore.set(0);
        set_request_brick.set(0);
        set_request_lumber.set(0);
        set_request_wool.set(0);
        set_request_grain.set(0);
        set_request_ore.set(0);
    };

    let submit_trade = move |_| {
        let offer = shared::Resources {
            brick: offer_brick.get(),
            lumber: offer_lumber.get(),
            wool: offer_wool.get(),
            grain: offer_grain.get(),
            ore: offer_ore.get(),
        };
        let request = shared::Resources {
            brick: request_brick.get(),
            lumber: request_lumber.get(),
            wool: request_wool.get(),
            grain: request_grain.get(),
            ore: request_ore.get(),
        };
        state.send(ClientRequest::TradeOffer {
            target_player_id: None, // Open to all players
            offer,
            request,
        });
        reset_form();
        set_show_form.set(false);
    };

    let cancel_my_trade = move |_| {
        if let Some(offer_id) = state.my_pending_trade.get() {
            state.send(ClientRequest::CancelTrade { offer_id });
        }
    };

    // Resource counter component
    let resource_counter = move |name: &'static str, color: &'static str,
                                  value: ReadSignal<u8>, setter: WriteSignal<u8>,
                                  max: u8, _is_offer: bool| {
        // Define closures outside the view! macro to avoid parsing issues with >= and <=
        let dec_disabled = move || value.get() == 0;
        let inc_disabled = {
            let max = max;
            move || value.get() >= max
        };
        let on_dec = move |_| {
            if value.get() > 0 {
                setter.set(value.get() - 1);
            }
        };
        let on_inc = {
            let max = max;
            move |_| {
                if value.get() < max {
                    setter.set(value.get() + 1);
                }
            }
        };

        view! {
            <div class="flex items-center justify-between text-[10px]">
                <span class=format!("font-bold {}", color)>{name}</span>
                <div class="flex items-center gap-1">
                    <button
                        class="flex-shrink-0 w-5 h-5 bg-red-700 hover:bg-red-600 rounded text-white font-bold text-xs disabled:opacity-30"
                        on:click=on_dec
                        disabled=dec_disabled
                    >"-"</button>
                    <span class="w-4 text-center text-white flex-shrink-0">{move || value.get()}</span>
                    <button
                        class="flex-shrink-0 w-5 h-5 bg-green-700 hover:bg-green-600 rounded text-white font-bold text-xs disabled:opacity-30"
                        on:click=on_inc
                        disabled=inc_disabled
                    >"+"</button>
                </div>
            </div>
        }
    };

    view! {
        <div class="flex flex-col space-y-2 min-h-0 overflow-y-auto">
            // Show pending trade if I have one
            <Show when=move || state.my_pending_trade.get().is_some()>
                <div class="bg-yellow-900/30 border border-yellow-700/50 rounded p-2 text-[10px]">
                    <div class="text-yellow-400 font-bold mb-1">"Your trade offer is pending..."</div>
                    <button
                        class="w-full py-1 bg-red-700 hover:bg-red-600 rounded text-white font-bold"
                        on:click=cancel_my_trade
                    >
                        "CANCEL TRADE"
                    </button>
                </div>
            </Show>

            // Show create trade button (only if no pending trade)
            <Show when=move || state.my_pending_trade.get().is_none() && !show_form.get()>
                <button
                    class="w-full py-2 bg-blue-700 hover:bg-blue-600 rounded text-[10px] font-bold transition-colors"
                    on:click=move |_| set_show_form.set(true)
                >
                    "PROPOSE TRADE"
                </button>
            </Show>

            // Trade creation form
            <Show when=move || show_form.get()>
                <div class="bg-slate-800/50 rounded p-2 space-y-2">
                    <div class="text-[9px] text-slate-400 font-bold">"YOU GIVE:"</div>
                    <div class="space-y-1">
                        {resource_counter("Brick", "text-red-400", offer_brick, set_offer_brick, state.my_resources.get().brick, true)}
                        {resource_counter("Wood", "text-green-400", offer_lumber, set_offer_lumber, state.my_resources.get().lumber, true)}
                        {resource_counter("Sheep", "text-lime-400", offer_wool, set_offer_wool, state.my_resources.get().wool, true)}
                        {resource_counter("Wheat", "text-yellow-400", offer_grain, set_offer_grain, state.my_resources.get().grain, true)}
                        {resource_counter("Ore", "text-slate-300", offer_ore, set_offer_ore, state.my_resources.get().ore, true)}
                    </div>

                    <div class="text-[9px] text-slate-400 font-bold mt-2">"YOU WANT:"</div>
                    <div class="space-y-1">
                        {resource_counter("Brick", "text-red-400", request_brick, set_request_brick, 19, false)}
                        {resource_counter("Wood", "text-green-400", request_lumber, set_request_lumber, 19, false)}
                        {resource_counter("Sheep", "text-lime-400", request_wool, set_request_wool, 19, false)}
                        {resource_counter("Wheat", "text-yellow-400", request_grain, set_request_grain, 19, false)}
                        {resource_counter("Ore", "text-slate-300", request_ore, set_request_ore, 19, false)}
                    </div>

                    <div class="flex gap-1 mt-2">
                        <button
                            class="flex-1 py-1 bg-slate-700 hover:bg-slate-600 rounded text-[10px] font-bold"
                            on:click=move |_| {
                                reset_form();
                                set_show_form.set(false);
                            }
                        >
                            "CANCEL"
                        </button>
                        <button
                            class="flex-1 py-1 bg-green-700 hover:bg-green-600 rounded text-[10px] font-bold disabled:opacity-40"
                            disabled=move || !is_valid_offer()
                            on:click=submit_trade
                        >
                            "SEND"
                        </button>
                    </div>
                </div>
            </Show>

            // Incoming trade offers
            <Show when=move || !state.incoming_trades.get().is_empty()>
                <div class="space-y-2">
                    <div class="text-[9px] text-slate-400 font-bold">"INCOMING TRADES:"</div>
                    <For
                        each=move || state.incoming_trades.get()
                        key=|trade| trade.offer_id
                        children=move |trade: crate::state::PendingTradeOffer| {
                            view! {
                                <IncomingTradeItem trade=trade />
                            }
                        }
                    />
                </div>
            </Show>
        </div>
    }
}

/// Trade timeout in seconds
const TRADE_TIMEOUT_SECONDS: f64 = 30.0;

#[component]
fn IncomingTradeItem(trade: crate::state::PendingTradeOffer) -> impl IntoView {
    let state = use_context::<GameState>().expect("GameState missing");
    let offer_id = trade.offer_id;
    let received_at = trade.received_at;

    // Timer state - seconds remaining
    let (seconds_left, set_seconds_left) = create_signal(TRADE_TIMEOUT_SECONDS);

    // Set up timer interval to update countdown
    create_effect(move |prev_handle: Option<Option<leptos_dom::helpers::IntervalHandle>>| {
        // Clean up previous interval if it exists
        if let Some(Some(handle)) = prev_handle {
            handle.clear();
        }

        // Start a new interval that updates every second
        let handle = set_interval_with_handle(
            move || {
                let now = js_sys::Date::now();
                let elapsed = (now - received_at) / 1000.0; // Convert ms to seconds
                let remaining = TRADE_TIMEOUT_SECONDS - elapsed;

                if remaining < 0.0 {
                    // Auto-decline the trade
                    set_seconds_left.set(0.0);
                    state.send(ClientRequest::TradeResponse { offer_id, accept: false });
                } else {
                    set_seconds_left.set(remaining);
                }
            },
            std::time::Duration::from_secs(1),
        );

        handle.ok()
    });

    let proposer_name = state.players.get()
        .iter()
        .find(|p| p.player_id == trade.proposer_id)
        .map(|p| p.name.clone())
        .unwrap_or_else(|| format!("Player {}", trade.proposer_id));

    // Check if I can afford what they're requesting
    let can_afford = move || {
        let res = state.my_resources.get();
        trade.requesting.brick <= res.brick &&
        trade.requesting.lumber <= res.lumber &&
        trade.requesting.wool <= res.wool &&
        trade.requesting.grain <= res.grain &&
        trade.requesting.ore <= res.ore
    };

    // Format resources
    let format_resources = |r: &shared::Resources| -> String {
        let mut parts = Vec::new();
        if r.brick > 0 { parts.push(format!("{}x Brick", r.brick)); }
        if r.lumber > 0 { parts.push(format!("{}x Wood", r.lumber)); }
        if r.wool > 0 { parts.push(format!("{}x Sheep", r.wool)); }
        if r.grain > 0 { parts.push(format!("{}x Wheat", r.grain)); }
        if r.ore > 0 { parts.push(format!("{}x Ore", r.ore)); }
        if parts.is_empty() { "nothing".to_string() } else { parts.join(" ") }
    };

    let offering_str = format_resources(&trade.offering);
    let requesting_str = format_resources(&trade.requesting);

    // Calculate progress bar width (percentage remaining)
    let progress_width = move || {
        let remaining = seconds_left.get();
        let percentage = (remaining / TRADE_TIMEOUT_SECONDS) * 100.0;
        format!("{}%", percentage.max(0.0))
    };

    // Timer color based on remaining time
    let timer_color = move || {
        let remaining = seconds_left.get();
        if remaining <= 5.0 {
            "bg-red-600"
        } else if remaining <= 10.0 {
            "bg-yellow-600"
        } else {
            "bg-blue-600"
        }
    };

    view! {
        <div class="bg-blue-900/30 border border-blue-700/50 rounded p-2 text-[9px] relative overflow-hidden">
            // Timer progress bar background
            <div class="absolute bottom-0 left-0 right-0 h-1 bg-slate-700">
                <div
                    class=move || format!("h-full transition-all duration-1000 {}", timer_color())
                    style=move || format!("width: {}", progress_width())
                ></div>
            </div>

            <div class="flex justify-between items-center mb-1">
                <div class="font-bold text-blue-300">{proposer_name}</div>
                <div class=move || {
                    let remaining = seconds_left.get();
                    if remaining <= 5.0 {
                        "text-red-400 font-bold"
                    } else if remaining <= 10.0 {
                        "text-yellow-400 font-bold"
                    } else {
                        "text-slate-400"
                    }
                }>
                    {move || format!("{}s", seconds_left.get().ceil() as i32)}
                </div>
            </div>
            <div class="text-slate-300">
                <span class="text-green-400">"Gives: "</span>{offering_str}
            </div>
            <div class="text-slate-300">
                <span class="text-red-400">"Wants: "</span>{requesting_str}
            </div>
            <div class="flex gap-1 mt-2">
                <button
                    class="flex-1 py-1 bg-red-700 hover:bg-red-600 rounded font-bold"
                    on:click=move |_| {
                        state.send(ClientRequest::TradeResponse { offer_id, accept: false });
                    }
                >
                    "DECLINE"
                </button>
                <button
                    class="flex-1 py-1 bg-green-700 hover:bg-green-600 rounded font-bold disabled:opacity-40"
                    disabled=move || !can_afford()
                    on:click=move |_| {
                        state.send(ClientRequest::TradeResponse { offer_id, accept: true });
                    }
                >
                    "ACCEPT"
                </button>
            </div>
        </div>
    }
}

#[component]
fn YearOfPlentyModal() -> impl IntoView {
    let state = use_context::<GameState>().expect("GameState missing");

    let (pick1, set_pick1) = create_signal::<Option<shared::ResourceType>>(None);
    let (pick2, set_pick2) = create_signal::<Option<shared::ResourceType>>(None);

    // Reset the modal selections when it closes
    create_effect(move |_| {
        if !state.year_of_plenty_pending.get() {
            set_pick1.set(None);
            set_pick2.set(None);
        }
    });

    let can_confirm = move || pick1.get().is_some() && pick2.get().is_some();

    let confirm = move |_| {
        if let (Some(r1), Some(r2)) = (pick1.get(), pick2.get()) {
            state.send(ClientRequest::YearOfPlentyChoice {
                resource1: r1,
                resource2: r2,
            });

            // UX: optimistically close/reset; server will also send YearOfPlentyResourcesReceived
            set_pick1.set(None);
            set_pick2.set(None);
        }
    };

    let resource_btn = move |res: shared::ResourceType, label: &'static str, color: &'static str| {
        let selected1 = Signal::derive(move || pick1.get() == Some(res));
        let selected2 = Signal::derive(move || pick2.get() == Some(res));

        let select = move |_| {
            match (pick1.get(), pick2.get()) {
                (None, _) => set_pick1.set(Some(res)),
                (Some(_), None) => set_pick2.set(Some(res)),
                (Some(_), Some(_)) => {
                    // If both filled, replace second (simple UX)
                    set_pick2.set(Some(res));
                }
            }
        };

        view! {
            <button
                class=move || {
                    let base = "px-3 py-2 rounded-lg border text-[11px] font-bold transition-all";
                    let is_selected = selected1.get() || selected2.get();
                    if is_selected {
                        format!("{base} bg-green-700/40 border-green-500 text-white")
                    } else {
                        format!("{base} bg-slate-800/60 border-slate-700 text-slate-200 hover:bg-slate-700/60")
                    }
                }
                on:click=select
                title=label
            >
                <span class=color>{label}</span>
            </button>
        }
    };

    let clear1 = move |_| set_pick1.set(None);
    let clear2 = move |_| set_pick2.set(None);

    let pick_label = move |p: Option<shared::ResourceType>| -> &'static str {
        match p {
            Some(shared::ResourceType::Brick) => "Brick",
            Some(shared::ResourceType::Wood) => "Wood",
            Some(shared::ResourceType::Sheep) | Some(shared::ResourceType::Wool) => "Sheep",
            Some(shared::ResourceType::Wheat) => "Wheat",
            Some(shared::ResourceType::Ore) => "Ore",
            _ => "—",
        }
    };

    view! {
        <div class="fixed inset-0 bg-black/80 flex items-center justify-center z-[9999] backdrop-blur-sm pointer-events-auto">
            <div class="bg-slate-900 border-2 border-emerald-600 rounded-2xl p-6 max-w-md w-full mx-4 shadow-2xl">
                <div class="flex justify-between items-start mb-2">
                    <div>
                        <h2 class="text-2xl font-bold text-emerald-400">"🌾 Year of Plenty"</h2>
                        <p class="text-slate-300 text-sm">"Choose 2 resources to receive from the bank."</p>
                    </div>
                    <div class="text-[10px] text-slate-400">
                        <div class="flex items-center gap-2">
                            <span class="font-mono">"1:"</span>
                            <span class="font-bold text-slate-200">{move || pick_label(pick1.get())}</span>
                            <button class="text-slate-400 hover:text-white" on:click=clear1 title="Clear pick 1">"×"</button>
                        </div>
                        <div class="flex items-center gap-2">
                            <span class="font-mono">"2:"</span>
                            <span class="font-bold text-slate-200">{move || pick_label(pick2.get())}</span>
                            <button class="text-slate-400 hover:text-white" on:click=clear2 title="Clear pick 2">"×"</button>
                        </div>
                    </div>
                </div>

                <div class="grid grid-cols-2 gap-2 mt-4">
                    {resource_btn(shared::ResourceType::Brick, "Brick", "text-red-400")}
                    {resource_btn(shared::ResourceType::Wood, "Wood", "text-green-400")}
                    {resource_btn(shared::ResourceType::Sheep, "Sheep", "text-lime-300")}
                    {resource_btn(shared::ResourceType::Wheat, "Wheat", "text-yellow-300")}
                    {resource_btn(shared::ResourceType::Ore, "Ore", "text-slate-300")}
                </div>

                <div class="flex justify-end gap-2 pt-4 mt-4 border-t border-slate-800">
                    <button
                        class="px-3 py-2 rounded-lg bg-slate-800 hover:bg-slate-700 border border-slate-700 text-[11px] font-bold text-slate-200"
                        on:click=move |_| { set_pick1.set(None); set_pick2.set(None); }
                    >
                        "Reset"
                    </button>
                    <button
                        class="px-3 py-2 rounded-lg bg-emerald-700 hover:bg-emerald-600 text-[11px] font-bold text-white disabled:opacity-40 disabled:cursor-not-allowed"
                        disabled=move || !can_confirm()
                        on:click=confirm
                    >
                        "Confirm"
                    </button>
                </div>
            </div>
        </div>
    }
}

#[component]
fn MonopolyModal() -> impl IntoView {
    let state = use_context::<GameState>().expect("GameState missing");

    let (selected, set_selected) = create_signal::<Option<shared::ResourceType>>(None);

    // Reset selection when modal closes
    create_effect(move |_| {
        if !state.monopoly_pending.get() {
            set_selected.set(None);
        }
    });

    let confirm = move |_| {
        if let Some(resource) = selected.get() {
            state.send(ClientRequest::MonopolyChoice { resource });
            set_selected.set(None);
        }
    };

    let resource_btn = move |res: shared::ResourceType, label: &'static str, color: &'static str| {
        let is_selected = Signal::derive(move || selected.get() == Some(res));

        view! {
            <button
                class=move || {
                    let base = "px-4 py-3 rounded-lg border text-sm font-bold transition-all";
                    if is_selected.get() {
                        format!("{base} bg-purple-700/40 border-purple-500 text-white")
                    } else {
                        format!("{base} bg-slate-800/60 border-slate-700 text-slate-200 hover:bg-slate-700/60")
                    }
                }
                on:click=move |_| set_selected.set(Some(res))
                title=label
            >
                <span class=color>{label}</span>
            </button>
        }
    };

    view! {
        <div class="fixed inset-0 bg-black/80 flex items-center justify-center z-[9999] backdrop-blur-sm pointer-events-auto">
            <div class="bg-slate-900 border-2 border-purple-600 rounded-2xl p-6 max-w-md w-full mx-4 shadow-2xl">
                <div class="mb-4">
                    <h2 class="text-2xl font-bold text-purple-400">"Monopoly"</h2>
                    <p class="text-slate-300 text-sm">"Choose a resource to steal from ALL other players."</p>
                </div>

                <div class="grid grid-cols-2 gap-2">
                    {resource_btn(shared::ResourceType::Brick, "Brick", "text-red-400")}
                    {resource_btn(shared::ResourceType::Wood, "Wood", "text-green-400")}
                    {resource_btn(shared::ResourceType::Sheep, "Sheep", "text-lime-300")}
                    {resource_btn(shared::ResourceType::Wheat, "Wheat", "text-yellow-300")}
                    {resource_btn(shared::ResourceType::Ore, "Ore", "text-slate-300")}
                </div>

                <div class="flex justify-end pt-4 mt-4 border-t border-slate-800">
                    <button
                        class="px-4 py-2 rounded-lg bg-purple-700 hover:bg-purple-600 text-sm font-bold text-white disabled:opacity-40 disabled:cursor-not-allowed"
                        disabled=move || selected.get().is_none()
                        on:click=confirm
                    >
                        "Confirm"
                    </button>
                </div>
            </div>
        </div>
    }
}