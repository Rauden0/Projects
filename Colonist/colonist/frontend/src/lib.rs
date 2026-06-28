use leptos::*;
use wasm_bindgen::prelude::wasm_bindgen;
use crate::pages::game::GamePage;
use crate::pages::menu::MainMenu;

mod state;
mod components;
mod pages;

use crate::state::{provide_game_state, GameState};
#[component]
pub fn App() -> impl IntoView {
    provide_game_state();
    let state = use_context::<GameState>().expect("GameState missing");

    view! {
        <main class="bg-slate-950 min-h-screen text-slate-200">
            <Show
                when=move || state.is_in_game.get()
                fallback=move || view! { <MainMenu /> }
            >
                <GamePage />
            </Show>
        </main>
    }
}

#[wasm_bindgen(start)]
pub fn run() {

    mount_to_body(App);
}