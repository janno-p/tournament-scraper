[<RequireQualifiedAccess>]
module TournamentScraper.Templates.Stylebook

open Oxpecker.ViewEngine

type appLink() =
    inherit a(class' = "font-medium text-blue-600 dark:text-blue-500 hover:underline focus:ring-4 focus:outline-none")

module private AppButton =
    let buttonClass = "relative inline-flex items-center justify-center p-0.5 mb-2 me-2 overflow-hidden text-sm font-medium text-gray-900 rounded-lg group bg-gradient-to-br"
    let buttonContentClass = "relative px-5 py-2.5 transition-all ease-in duration-75 bg-white dark:bg-gray-900 rounded-md group-hover:bg-opacity-0"

let buttonDefault (contents: HtmlElement) =
    button(class' = $"{AppButton.buttonClass} from-purple-600 to-blue-500 group-hover:from-purple-600 group-hover:to-blue-500 hover:text-white dark:text-white focus:ring-blue-300 dark:focus:ring-blue-800") {
        span(class' = AppButton.buttonContentClass) {
            contents
        }
    }

(*
<button class=" from-cyan-500 to-blue-500 group-hover:from-cyan-500 group-hover:to-blue-500 hover:text-white dark:text-white focus:ring-cyan-200 dark:focus:ring-cyan-800">
<span class="relative px-5 py-2.5 transition-all ease-in duration-75 bg-white dark:bg-gray-900 rounded-md group-hover:bg-opacity-0">
Cyan to blue
</span>
</button>
<button class=" from-green-400 to-blue-600 group-hover:from-green-400 group-hover:to-blue-600 hover:text-white dark:text-white focus:ring-green-200 dark:focus:ring-green-800">
<span class="relative px-5 py-2.5 transition-all ease-in duration-75 bg-white dark:bg-gray-900 rounded-md group-hover:bg-opacity-0">
Green to blue
</span>
</button>
<button class=" from-purple-500 to-pink-500 group-hover:from-purple-500 group-hover:to-pink-500 hover:text-white dark:text-white focus:ring-purple-200 dark:focus:ring-purple-800">
<span class="relative px-5 py-2.5 transition-all ease-in duration-75 bg-white dark:bg-gray-900 rounded-md group-hover:bg-opacity-0">
Purple to pink
</span>
</button>
<button class=" from-pink-500 to-orange-400 group-hover:from-pink-500 group-hover:to-orange-400 hover:text-white dark:text-white focus:ring-pink-200 dark:focus:ring-pink-800">
<span class="relative px-5 py-2.5 transition-all ease-in duration-75 bg-white dark:bg-gray-900 rounded-md group-hover:bg-opacity-0">
Pink to orange
</span>
</button>
<button class=" from-teal-300 to-lime-300 group-hover:from-teal-300 group-hover:to-lime-300 dark:text-white dark:hover:text-gray-900 focus:ring-lime-200 dark:focus:ring-lime-800">
<span class="relative px-5 py-2.5 transition-all ease-in duration-75 bg-white dark:bg-gray-900 rounded-md group-hover:bg-opacity-0">
Teal to Lime
</span>
</button>
<button class=" from-red-200 via-red-300 to-yellow-200 group-hover:from-red-200 group-hover:via-red-300 group-hover:to-yellow-200 dark:text-white dark:hover:text-gray-900 focus:ring-red-100 dark:focus:ring-red-400">
<span class="relative px-5 py-2.5 transition-all ease-in duration-75 bg-white dark:bg-gray-900 rounded-md group-hover:bg-opacity-0">
Red to Yellow
</span>
</button>
*)
