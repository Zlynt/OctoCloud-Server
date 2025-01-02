import { createBrowserRouter, createRoutesFromElements, Route } from 'react-router-dom';
import Home from './app/routes/Home';

const Router = createBrowserRouter(
  createRoutesFromElements(
    <>
      <Route path="/" element={<Home />} />
    </>
  )
)

export default Router;
