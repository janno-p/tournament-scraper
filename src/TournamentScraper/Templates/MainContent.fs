module MainContent

open Giraffe.ViewEngine

let layout (content: XmlNode list) =
    main [_class "max-w-screen-xl mx-auto p-4"] content
