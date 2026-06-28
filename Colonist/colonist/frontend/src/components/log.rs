use leptos::*;
use crate::state::GameState;

#[component]
pub fn GameLog() -> impl IntoView {
    let state = use_context::<GameState>().expect("Context missing");

    view! {
        <div class="bg-gray-800 p-4 rounded-lg h-64 overflow-y-auto">
            <h3 class="text-orange-400 font-bold mb-2">"HERNÍ LOG"</h3>
            <div class="flex flex-col-reverse">
                <For
                    each=move || state.messages.get()
                    key=|m| m.clone()
                    children=|m| view! { <p class="text-sm border-b border-gray-700 py-1 italic">"> " {m}</p> }
                />
            </div>
        </div>
    }
}